using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.SavingsGoals.Commands.UpdateSavingsGoal;

public class UpdateSavingsGoalHandler : ICommandHandler<UpdateSavingsGoalCommand, SavingsGoalDto>
{
    private readonly ISavingsGoalRepository _goals;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSavingsGoalHandler(
        ISavingsGoalRepository goals,
        IAccountRepository accounts,
        IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<SavingsGoalDto> Handle(UpdateSavingsGoalCommand request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("SavingsGoal", request.Id);

        if (request.LinkedAccountId is not null && request.LinkedAccountId != goal.LinkedAccountId)
        {
            var account = await _accounts.GetByIdForUserAsync(request.LinkedAccountId.Value, request.UserId, ct)
                ?? throw new NotFoundException("Account", request.LinkedAccountId.Value);

            if (account.Type != AccountType.Savings)
                throw new DomainException("A goal can only be linked to a savings account.");

            if (account.Balance.Currency != goal.TargetAmount.Currency)
                throw new DomainException("The linked account must use the goal's currency.");
        }

        goal.UpdateDetails(
            request.Name,
            new Money(request.TargetAmount, goal.TargetAmount.Currency),
            request.Deadline,
            request.LinkedAccountId);

        await _unitOfWork.SaveChangesAsync(ct);

        return SavingsGoalDto.FromEntity(goal);
    }
}
