using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Accounts.Commands.DepositToAccount;

public class DepositToAccountHandler : ICommandHandler<DepositToAccountCommand, AccountMovementDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IUnitOfWork _unitOfWork;

    public DepositToAccountHandler(
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _movements = movements;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountMovementDto> Handle(DepositToAccountCommand request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.AccountId, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.AccountId);

        // Amount is denominated in the account's own currency: manual deposits
        // can't introduce a currency mismatch.
        var amount = new Money(request.Amount, account.Balance.Currency);
        AccountMovement movement = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            account.Deposit(amount);

            movement = new AccountMovement(
                account.Id,
                request.UserId,
                (MovementType)request.Type,
                amount,
                account.Balance,
                request.Description,
                DateTime.UtcNow);

            await _movements.AddAsync(movement, ct);
        }, ct);

        return AccountMovementDto.FromEntity(movement);
    }
}