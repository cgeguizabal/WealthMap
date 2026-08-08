using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;

namespace WealthMap.Application.Features.ProductGoals.Commands.ContributeToProductGoal;

public record ContributeToProductGoalCommand(
    Guid GoalId,
    Guid UserId,
    decimal Amount) : ICommand<ProductGoalDto>;
