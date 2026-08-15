using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Application.Features.CreditCards.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.CreditCards.Commands.PayCreditCard;

public class PayCreditCardHandler : ICommandHandler<PayCreditCardCommand, CardPaymentResultDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IPaymentRepository _payments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly CardStatementLoader _statements;

    public PayCreditCardHandler(
        ICreditCardRepository cards,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IPaymentRepository payments,
        IUnitOfWork unitOfWork,
        IInstallmentPurchaseRepository installments,
        CardStatementLoader statements)
    {
        _cards = cards;
        _accounts = accounts;
        _movements = movements;
        _payments = payments;
        _installments = installments;
        _statements = statements;
        _unitOfWork = unitOfWork;
    }

    public async Task<CardPaymentResultDto> Handle(PayCreditCardCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.CardId, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.CardId);

        var amount = new Money(request.Amount, card.CreditLimit.Currency);
        var occurredAt = DateTime.UtcNow;

        // A plan's installment for the month is part of the card's statement, so
        // paying that statement settles it. Without this the schedule would sit
        // unchanged while the balance it belongs to fell, and "8 months left" would
        // slowly stop being true.
        var plansOnCard = await LoadPlansForCardAsync(card.Id, request.UserId, ct);
        var settled = new List<(InstallmentPurchase Plan, InstallmentPayment Paid)>();

        // External source: cash or someone else paid — the debt shrinks without
        // touching any tracked account, so no movement is written. The payment row
        // is what keeps the payment on the record at all.
        if (request.SourceType.Equals("External", StringComparison.OrdinalIgnoreCase))
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                card.RegisterPayment(amount);
                settled.AddRange(SettleInstallments(card, plansOnCard, amount));

                await _payments.AddAsync(new Payment(
                    request.UserId,
                    PaymentTargetType.CreditCard,
                    card.Id,
                    amount,
                    PaymentSourceType.External,
                    null,
                    occurredAt,
                    request.Notes), ct);
            }, ct);

            return new CardPaymentResultDto(
                await _statements.ToDtoAsync(card, request.UserId, ct), null, Describe(settled));
        }

        var account = await _accounts.GetByIdForUserAsync(request.SourceAccountId!.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.SourceAccountId.Value);

        AccountMovement movement = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            account.Withdraw(amount);
            card.RegisterPayment(amount);
            settled.AddRange(SettleInstallments(card, plansOnCard, amount));

            movement = new AccountMovement(
                account.Id,
                request.UserId,
                MovementType.Payment,
                amount,
                account.Balance,
                $"Payment to card '{card.CardName}'",
                occurredAt,
                relatedEntityId: card.Id);

            await _movements.AddAsync(movement, ct);

            await _payments.AddAsync(new Payment(
                request.UserId,
                PaymentTargetType.CreditCard,
                card.Id,
                amount,
                PaymentSourceType.Account,
                account.Id,
                occurredAt,
                request.Notes), ct);
        }, ct);

        return new CardPaymentResultDto(
            await _statements.ToDtoAsync(card, request.UserId, ct),
            AccountMovementDto.FromEntity(movement),
            Describe(settled));
    }

    private async Task<IReadOnlyList<InstallmentPurchase>> LoadPlansForCardAsync(
        Guid cardId, Guid userId, CancellationToken ct)
    {
        var plans = await _installments.GetAllForUserAsync(userId, ct);

        return plans.Where(p => p.CreditCardId == cardId && !p.IsCompleted).ToList();
    }

    /// <summary>
    /// Spends the payment on whatever installments this statement had already
    /// billed, oldest due date first across every plan on the card.
    /// </summary>
    /// <remarks>
    /// No <c>Payment</c> row is written for these. The card payment is the money
    /// leaving the account, and recording it twice would double every total that
    /// sums the ledger. What changes here is the schedule, not the cash.
    ///
    /// The card balance is not reduced again either: <c>RegisterPayment</c> has
    /// already taken the full amount off <c>UsedCredit</c>, and an installment
    /// being marked paid is the consequence of that, not a second payment.
    /// </remarks>
    private static List<(InstallmentPurchase Plan, InstallmentPayment Paid)> SettleInstallments(
        CreditCard card, IReadOnlyList<InstallmentPurchase> plans, Money amount)
    {
        var settled = new List<(InstallmentPurchase, InstallmentPayment)>();

        if (plans.Count == 0) return settled;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueThrough = LiquidityProjection.StatementDueDate(
            today, card.StatementCutoffDay, card.PaymentDueDay);

        var remaining = amount;

        // Ordered by what falls due first, so a payment too small to cover every
        // plan settles the most urgent rather than whichever was created first.
        foreach (var plan in plans.OrderBy(p => p.NextUnpaid().DueDate))
        {
            if (remaining.Amount <= 0) break;

            foreach (var paid in plan.SettleDueThrough(dueThrough, remaining))
            {
                remaining -= paid.Amount;
                settled.Add((plan, paid));
            }
        }

        return settled;
    }

    private static IReadOnlyList<SettledInstallmentDto> Describe(
        List<(InstallmentPurchase Plan, InstallmentPayment Paid)> settled) =>
        settled
            .Select(s => new SettledInstallmentDto(
                s.Plan.Id, s.Plan.ProductName, s.Paid.Number, s.Plan.MonthsCount, s.Paid.Amount.Amount))
            .ToList();
}