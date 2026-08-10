using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.InstallmentPurchases.Commands.PayInstallment;

public class PayInstallmentHandler : ICommandHandler<PayInstallmentCommand, InstallmentPaymentResultDto>
{
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly ICreditCardRepository _cards;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IPaymentRepository _payments;
    private readonly IUnitOfWork _unitOfWork;

    public PayInstallmentHandler(
        IInstallmentPurchaseRepository installments,
        ICreditCardRepository cards,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IPaymentRepository payments,
        IUnitOfWork unitOfWork)
    {
        _installments = installments;
        _cards = cards;
        _accounts = accounts;
        _movements = movements;
        _payments = payments;
        _unitOfWork = unitOfWork;
    }

    public async Task<InstallmentPaymentResultDto> Handle(PayInstallmentCommand request, CancellationToken ct)
    {
        var purchase = await _installments.GetByIdForUserAsync(request.InstallmentPurchaseId, request.UserId, ct)
            ?? throw new NotFoundException("InstallmentPurchase", request.InstallmentPurchaseId);

        var card = await _cards.GetByIdForUserAsync(purchase.CreditCardId, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", purchase.CreditCardId);

        var occurredAt = DateTime.UtcNow;

        if (request.SourceType.Equals("External", StringComparison.OrdinalIgnoreCase))
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var paid = purchase.PayNextInstallment();
                card.RegisterPayment(paid.Amount);

                await _payments.AddAsync(new Payment(
                    request.UserId,
                    PaymentTargetType.Installment,
                    purchase.Id,
                    paid.Amount,
                    PaymentSourceType.External,
                    null,
                    occurredAt,
                    request.Notes ?? $"Installment {paid.Number}/{purchase.MonthsCount}"), ct);
            }, ct);

            return new InstallmentPaymentResultDto(
                InstallmentPurchaseDto.FromEntity(purchase), null);
        }

        var account = await _accounts.GetByIdForUserAsync(request.SourceAccountId!.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.SourceAccountId.Value);

        AccountMovement movement = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var paid = purchase.PayNextInstallment();

            account.Withdraw(paid.Amount);
            card.RegisterPayment(paid.Amount);

            movement = new AccountMovement(
                account.Id,
                request.UserId,
                MovementType.Payment,
                paid.Amount,
                account.Balance,
                $"Installment {paid.Number}/{purchase.MonthsCount}: {purchase.ProductName}",
                occurredAt,
                relatedEntityId: purchase.Id);

            await _movements.AddAsync(movement, ct);

            await _payments.AddAsync(new Payment(
                request.UserId,
                PaymentTargetType.Installment,
                purchase.Id,
                paid.Amount,
                PaymentSourceType.Account,
                account.Id,
                occurredAt,
                request.Notes ?? $"Installment {paid.Number}/{purchase.MonthsCount}"), ct);
        }, ct);

        return new InstallmentPaymentResultDto(
            InstallmentPurchaseDto.FromEntity(purchase),
            AccountMovementDto.FromEntity(movement));
    }
}