using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.Commands.UpdateSavingsGoal;

public record UpdateSavingsGoalCommand(
    Guid Id,
    Guid UserId,
    string Name,
    decimal TargetAmount,
    DateOnly Deadline,
    Guid? LinkedAccountId) : ICommand<SavingsGoalDto>;
