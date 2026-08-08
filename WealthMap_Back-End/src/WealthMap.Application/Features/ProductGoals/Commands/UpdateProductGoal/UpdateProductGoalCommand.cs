using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;

namespace WealthMap.Application.Features.ProductGoals.Commands.UpdateProductGoal;

public record UpdateProductGoalCommand(
    Guid Id,
    Guid UserId,
    string Name,
    decimal TargetAmount,
    DateOnly? Deadline) : ICommand<ProductGoalDto>;
