using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.ProductGoals.DTOs;

public record ProductGoalDto(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    string Currency,
    DateOnly? Deadline,
    decimal ProgressPercentage,
    int? MonthsRemaining,
    decimal? RequiredMonthlyContribution,
    string Status,
    DateTime CreatedAt)
{
    public static ProductGoalDto FromEntity(ProductGoal goal) => new(
        goal.Id,
        goal.Name,
        goal.TargetAmount.Amount,
        goal.CurrentAmount.Amount,
        goal.TargetAmount.Currency,
        goal.Deadline,
        goal.ProgressPercentage,
        goal.MonthsRemaining,
        goal.RequiredMonthlyContribution?.Amount,
        goal.Status.ToString(),
        goal.CreatedAt);
}
