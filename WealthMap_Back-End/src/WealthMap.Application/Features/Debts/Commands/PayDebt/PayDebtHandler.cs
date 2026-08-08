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
    private readonly IUnitOfWork _unitOfWork;

    public PayDebtHandler(
        IDebtRepository debts,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IUnitOfWork unitOfWork)
    {
        _debts = debts;
        _accounts = accounts;
        _movements = movements;
        _unitOfWork = unitOfWork;
    }

    public async Task<DebtPaymentResultDto> Handle(PayDebtCommand request, CancellationToken ct)
    {
        var debt = await _debts.GetByIdForUserAsync(request.DebtId, request.UserId, ct)
            ?? throw new NotFoundException("Debt", request.DebtId);

        var amount = new Money(request.Amount, debt.OriginalAmount.Currency);

        if (request.SourceType.Equals("External", StringComparison.OrdinalIgnoreCase))
        {
            debt.RegisterPayment(amount);
            await _unitOfWork.SaveChangesAsync(ct);

            return new DebtPaymentResultDto(DebtDto.FromEntity(debt), null);
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
                DateTime.UtcNow,
                relatedEntityId: debt.Id);

            await _movements.AddAsync(movement, ct);
        }, ct);

        return new DebtPaymentResultDto(
            DebtDto.FromEntity(debt),
            AccountMovementDto.FromEntity(movement));
    }
}
