using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Application.Features.Debts.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Debts.Commands.PayDebt;

public class PayDebtHandler : ICommandHandler<PayDebtCommand, DebtPaymentResultDto>
{
    private readonly IDebtRepository _debts;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IPaymentRepository _payments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public PayDebtHandler(
        IDebtRepository debts,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IPaymentRepository payments,
        IUnitOfWork unitOfWork,
        IUserClock clock)
    {
        _debts = debts;
        _accounts = accounts;
        _movements = movements;
        _payments = payments;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<DebtPaymentResultDto> Handle(PayDebtCommand request, CancellationToken ct)
    {
        var debt = await _debts.GetByIdForUserAsync(request.DebtId, request.UserId, ct)
            ?? throw new NotFoundException("Debt", request.DebtId);

        var amount = new Money(request.Amount, debt.OriginalAmount.Currency);
        var occurredAt = DateTime.UtcNow;

        if (request.SourceType.Equals("External", StringComparison.OrdinalIgnoreCase))
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                debt.RegisterPayment(amount);

                await _payments.AddAsync(new Payment(
                    request.UserId,
                    PaymentTargetType.Debt,
                    debt.Id,
                    amount,
                    PaymentSourceType.External,
                    null,
                    occurredAt,
                    request.Notes), ct);
            }, ct);

            return new DebtPaymentResultDto(DebtDto.FromEntity(debt, _clock.Today), null);
        }

        var account = await _accounts.GetByIdForUserAsync(request.SourceAccountId!.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.SourceAccountId.Value);

        AccountMovement movement = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            account.Withdraw(amount);
            debt.RegisterPayment(amount);

            movement = new AccountMovement(
                account.Id,
                request.UserId,
                MovementType.Payment,
                amount,
                account.Balance,
                $"Payment on debt '{debt.Name}'",
                occurredAt,
                relatedEntityId: debt.Id);

            await _movements.AddAsync(movement, ct);

            await _payments.AddAsync(new Payment(
                request.UserId,
                PaymentTargetType.Debt,
                debt.Id,
                amount,
                PaymentSourceType.Account,
                account.Id,
                occurredAt,
                request.Notes), ct);
        }, ct);

        return new DebtPaymentResultDto(
            DebtDto.FromEntity(debt, _clock.Today),
            AccountMovementDto.FromEntity(movement));
    }
}