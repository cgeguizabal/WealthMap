using WealthMap.Application.Common.Models;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Dashboard.DTOs;

public record UpcomingDueDateDto(
    string Kind,
    Guid EntityId,
    string Name,
    DateOnly DueDate,
    int DaysUntil,
    decimal Amount);

public record GoalsSnapshotDto(
    int Total,
    int Completed,
    int BehindSchedule,
    decimal TotalTargeted,
    decimal TotalSaved);

public record DashboardDto(
    string Currency,
    decimal TotalAvailable,
    decimal TotalInChecking,
    decimal TotalInSavings,
    decimal TotalCreditLimit,
    decimal TotalUsedCredit,
    decimal TotalAvailableCredit,
    decimal TotalLoanDebt,
    decimal InstallmentRemaining,
    decimal TotalDebt,
    decimal NetWorth,
    decimal MonthlyNetIncome,
    decimal SpendableCash,
    decimal IncomingBeforeHorizon,
    decimal CommittedBeforeHorizon,
    DateOnly SafeToSpendHorizon,
    decimal MonthlyObligations,
    decimal SafeToSpend,
    decimal MonthSpending,
    decimal? DebtRatioPercentage,
    IReadOnlyList<UpcomingDueDateDto> UpcomingDueDates,
    GoalsSnapshotDto Goals,
    IReadOnlyList<string> ExcludedCurrencies)
{
    public static DashboardDto FromSnapshot(FinancialSnapshot s)
    {
        var savings = s.SavingsGoals;
        var products = s.ProductGoals;

        var goals = new GoalsSnapshotDto(
            savings.Count + products.Count,
            savings.Count(g => g.Status == GoalStatus.Completed)
                + products.Count(g => g.Status == GoalStatus.Completed),
            savings.Count(g => g.Status is GoalStatus.BehindSchedule or GoalStatus.DeadlinePassed)
                + products.Count(g => g.Status is GoalStatus.BehindSchedule or GoalStatus.DeadlinePassed),
            savings.Sum(g => g.TargetAmount.Amount) + products.Sum(g => g.TargetAmount.Amount),
            savings.Sum(g => g.CurrentAmount.Amount) + products.Sum(g => g.CurrentAmount.Amount));

        var due = s.UpcomingDueDates(30)
            .Select(d => new UpcomingDueDateDto(d.Kind, d.EntityId, d.Name, d.DueDate, d.DaysUntil, d.Amount))
            .ToList();

        return new DashboardDto(
            s.Currency,
            s.TotalAvailable.Amount,
            s.TotalInChecking.Amount,
            s.TotalInSavings.Amount,
            s.TotalCreditLimit.Amount,
            s.TotalUsedCredit.Amount,
            s.TotalAvailableCredit.Amount,
            s.TotalLoanDebt.Amount,
            s.InstallmentRemaining.Amount,
            s.TotalDebt.Amount,
            s.NetWorth.Amount,
            s.MonthlyNetIncome.Amount,
            s.SpendableCash.Amount,
            s.IncomingBeforeHorizon.Amount,
            s.CommittedBeforeHorizon.Amount,
            s.SafeToSpendHorizon,
            s.MonthlyObligations.Amount,
            s.SafeToSpend.Amount,
            s.MonthSpending.Amount,
            s.DebtRatio,
            due,
            goals,
            s.ExcludedCurrencies);
    }
}
