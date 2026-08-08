using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;

namespace WealthMap.Application.Features.ProductGoals.Commands.CreateProductGoal;

public record CreateProductGoalCommand(
    Guid UserId,
    string Name,
    decimal TargetAmount,
    string Currency,
    decimal? CurrentAmount,
    DateOnly? Deadline) : ICommand<ProductGoalDto>;
