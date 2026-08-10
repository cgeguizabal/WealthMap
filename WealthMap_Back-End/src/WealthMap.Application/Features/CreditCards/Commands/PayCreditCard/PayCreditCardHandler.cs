using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Application.Features.CreditCards.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.CreditCards.Commands.PayCreditCard;

public class PayCreditCardHandler : ICommandHandler<PayCreditCardCommand, CardPaymentResultDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IPaymentRepository _payments;
    private readonly IUnitOfWork _unitOfWork;

    public PayCreditCardHandler(
        ICreditCardRepository cards,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IPaymentRepository payments,
        IUnitOfWork unitOfWork)
    {
        _cards = cards;
        _accounts = accounts;
        _movements = movements;
        _payments = payments;
        _unitOfWork = unitOfWork;
    }

    public async Task<CardPaymentResultDto> Handle(PayCreditCardCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.CardId, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.CardId);

        var amount = new Money(request.Amount, card.CreditLimit.Currency);
        var occurredAt = DateTime.UtcNow;

        // External source: cash or someone else paid — the debt shrinks without
        // touching any tracked account, so no movement is written. The payment row
        // is what keeps the payment on the record at all.
        if (request.SourceType.Equals("External", StringComparison.OrdinalIgnoreCase))
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                card.RegisterPayment(amount);

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

            return new CardPaymentResultDto(CreditCardDto.FromEntity(card), null);
        }

        var account = await _accounts.GetByIdForUserAsync(request.SourceAccountId!.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.SourceAccountId.Value);

        AccountMovement movement = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            account.Withdraw(amount);
            card.RegisterPayment(amount);

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
            CreditCardDto.FromEntity(card),
            AccountMovementDto.FromEntity(movement));
    }
}