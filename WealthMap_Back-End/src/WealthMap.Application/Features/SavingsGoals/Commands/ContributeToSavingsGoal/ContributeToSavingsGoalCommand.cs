using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.Commands.ContributeToSavingsGoal;

public record ContributeToSavingsGoalCommand(
    Guid GoalId,
    Guid UserId,
    decimal Amount,
    Guid? SourceAccountId) : ICommand<SavingsGoalContributionResultDto>;
