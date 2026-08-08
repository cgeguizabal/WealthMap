using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.Commands.CreateSavingsGoal;

public record CreateSavingsGoalCommand(
    Guid UserId,
    string Name,
    decimal TargetAmount,
    string Currency,
    decimal? CurrentAmount,
    DateOnly Deadline,
    Guid? LinkedAccountId) : ICommand<SavingsGoalDto>;
