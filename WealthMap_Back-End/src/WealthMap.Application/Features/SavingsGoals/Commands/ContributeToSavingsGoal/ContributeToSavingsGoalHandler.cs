using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Application.Features.SavingsGoals.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.SavingsGoals.Commands.ContributeToSavingsGoal;

public class ContributeToSavingsGoalHandler
    : ICommandHandler<ContributeToSavingsGoalCommand, SavingsGoalContributionResultDto>
{
    private readonly ISavingsGoalRepository _goals;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IUnitOfWork _unitOfWork;

    public ContributeToSavingsGoalHandler(
        ISavingsGoalRepository goals,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _accounts = accounts;
        _movements = movements;
        _unitOfWork = unitOfWork;
    }

    public async Task<SavingsGoalContributionResultDto> Handle(
        ContributeToSavingsGoalCommand request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.GoalId, request.UserId, ct)
            ?? throw new NotFoundException("SavingsGoal", request.GoalId);

        var amount = new Money(request.Amount, goal.TargetAmount.Currency);

        // Unlinked goal: tracking only — no real money moves.
        if (goal.LinkedAccountId is null)
        {
            if (request.SourceAccountId is not null)
                throw new DomainException(
                    "This goal has no linked account; contributions are tracked only. Omit sourceAccountId.");

            goal.Contribute(amount);
            await _unitOfWork.SaveChangesAsync(ct);

            return new SavingsGoalContributionResultDto(SavingsGoalDto.FromEntity(goal), null);
        }

        // Linked goal: real transfer from a source account into the linked savings account.
        if (request.SourceAccountId is null)
            throw new DomainException(
                "This goal is linked to a savings account; provide sourceAccountId to transfer from.");

        if (request.SourceAccountId == goal.LinkedAccountId)
            throw new DomainException("Source account cannot be the goal's own linked account.");

        var source = await _accounts.GetByIdForUserAsync(request.SourceAccountId.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.SourceAccountId.Value);

        var linked = await _accounts.GetByIdForUserAsync(goal.LinkedAccountId.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", goal.LinkedAccountId.Value);

        AccountMovement outMovement = null!;
        var occurredAt = DateTime.UtcNow;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            source.Withdraw(amount);
            linked.Deposit(amount);
            goal.Contribute(amount);

            outMovement = new AccountMovement(
                source.Id,
                request.UserId,
                MovementType.TransferOut,
                amount,
                source.Balance,
                $"Contribution to goal '{goal.Name}'",
                occurredAt,
                relatedEntityId: goal.Id);

            var inMovement = new AccountMovement(
                linked.Id,
                request.UserId,
                MovementType.TransferIn,
                amount,
                linked.Balance,
                $"Contribution to goal '{goal.Name}'",
                occurredAt,
                relatedEntityId: goal.Id);

            await _movements.AddAsync(outMovement, ct);
            await _movements.AddAsync(inMovement, ct);
        }, ct);

        return new SavingsGoalContributionResultDto(
            SavingsGoalDto.FromEntity(goal),
            AccountMovementDto.FromEntity(outMovement));
    }
}
