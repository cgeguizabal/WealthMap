using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Accounts.Commands.TransferBetweenAccounts;

public class TransferBetweenAccountsHandler : ICommandHandler<TransferBetweenAccountsCommand, TransferResultDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IUnitOfWork _unitOfWork;

    public TransferBetweenAccountsHandler(
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _movements = movements;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferResultDto> Handle(TransferBetweenAccountsCommand request, CancellationToken ct)
    {
        var from = await _accounts.GetByIdForUserAsync(request.FromAccountId, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.FromAccountId);

        var to = await _accounts.GetByIdForUserAsync(request.ToAccountId, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.ToAccountId);

        var amount = new Money(request.Amount, from.Balance.Currency);
        var occurredAt = DateTime.UtcNow;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            from.Withdraw(amount);
            to.Deposit(amount);

            var outMovement = new AccountMovement(
                from.Id,
                request.UserId,
                MovementType.TransferOut,
                amount,
                from.Balance,
                request.Description ?? $"Transfer to '{to.Name}'",
                occurredAt,
                relatedEntityId: to.Id);

            var inMovement = new AccountMovement(
                to.Id,
                request.UserId,
                MovementType.TransferIn,
                amount,
                to.Balance,
                request.Description ?? $"Transfer from '{from.Name}'",
                occurredAt,
                relatedEntityId: from.Id);

            await _movements.AddAsync(outMovement, ct);
            await _movements.AddAsync(inMovement, ct);
        }, ct);

        return new TransferResultDto(
            AccountDto.FromEntity(from),
            AccountDto.FromEntity(to),
            amount.Amount,
            amount.Currency,
            occurredAt);
    }
}