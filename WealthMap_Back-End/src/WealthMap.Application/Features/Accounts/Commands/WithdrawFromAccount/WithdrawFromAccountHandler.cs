using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Accounts.Commands.WithdrawFromAccount;

public class WithdrawFromAccountHandler : ICommandHandler<WithdrawFromAccountCommand, AccountMovementDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IUnitOfWork _unitOfWork;

    public WithdrawFromAccountHandler(
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _movements = movements;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountMovementDto> Handle(WithdrawFromAccountCommand request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.AccountId, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.AccountId);

        var amount = new Money(request.Amount, account.Balance.Currency);
        AccountMovement movement = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            account.Withdraw(amount);

            // Cash leaving via ATM exits tracking by design (no cash wallet),
            // so every manual withdrawal is an AtmWithdrawal; location is optional.
            movement = new AccountMovement(
                account.Id,
                request.UserId,
                MovementType.AtmWithdrawal,
                amount,
                account.Balance,
                request.Description,
                DateTime.UtcNow,
                location: request.Location);

            await _movements.AddAsync(movement, ct);
        }, ct);

        return AccountMovementDto.FromEntity(movement);
    }
}