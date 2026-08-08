using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.SavingsGoals.DTOs;

public record SavingsGoalDto(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    string Currency,
    DateOnly Deadline,
    Guid? LinkedAccountId,
    decimal ProgressPercentage,
    int? MonthsRemaining,
    decimal? RequiredMonthlyContribution,
    string Status,
    DateTime CreatedAt)
{
    public static SavingsGoalDto FromEntity(SavingsGoal goal) => new(
        goal.Id,
        goal.Name,
        goal.TargetAmount.Amount,
        goal.CurrentAmount.Amount,
        goal.TargetAmount.Currency,
        goal.Deadline,
        goal.LinkedAccountId,
        goal.ProgressPercentage,
        goal.MonthsRemaining,
        goal.RequiredMonthlyContribution?.Amount,
        goal.Status.ToString(),
        goal.CreatedAt);
}
