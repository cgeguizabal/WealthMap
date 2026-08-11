using System.Globalization;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Alerts.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Alerts;

/// <summary>
/// The rule set from the product spec, evaluated against a snapshot.
/// Pure: same snapshot in, same alerts out.
/// </summary>
/// <remarks>
/// Every alert carries the parts it was composed from as well as the English
/// sentence. Amounts stay decimal and dates stay ISO — formatting them here
/// would bake in this server's culture, and the client formats every other
/// figure in the user's own.
/// </remarks>
public static class AlertRules
{
    public const decimal HighDebtRatioPercentage = 40m;

    /// <summary>Invariant so the client parses what it is given, whatever the server's culture.</summary>
    private static string Num(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Date(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static IReadOnlyList<AlertDto> Evaluate(FinancialSnapshot s)
    {
        var alerts = new List<AlertDto>();

        alerts.AddRange(CardsDueSoon(s));
        alerts.AddRange(InsufficientBalanceForCardPayments(s));
        alerts.AddRange(DebtsAndInstallmentsDueSoon(s));
        alerts.AddRange(HighDebtRatio(s));
        alerts.AddRange(Overspending(s));
        alerts.AddRange(GoalAlerts(s));

        return alerts
            .OrderByDescending(a => a.SeverityValue)
            .ToList();
    }

    private static IEnumerable<AlertDto> CardsDueSoon(FinancialSnapshot s) =>
        s.UpcomingDueDates(FinancialSnapshot.DueSoonDays)
            .Where(d => d.Kind == "CreditCard")
            .Select(d => AlertDto.Create(
                AlertType.CardPaymentDueSoon,
                d.DaysUntil <= 2 ? AlertSeverity.Critical : AlertSeverity.Warning,
                $"'{d.Name}' payment due in {d.DaysUntil} day(s)",
                $"You owe {d.Amount:N2} {s.Currency} on '{d.Name}', due {d.DueDate:yyyy-MM-dd}.",
                d.EntityId,
                new Dictionary<string, string>
                {
                    ["name"] = d.Name,
                    ["amount"] = Num(d.Amount),
                    ["currency"] = s.Currency,
                    ["dueDate"] = Date(d.DueDate),
                    ["daysUntil"] = Int(d.DaysUntil)
                }));

    private static IEnumerable<AlertDto> DebtsAndInstallmentsDueSoon(FinancialSnapshot s) =>
        s.UpcomingDueDates(FinancialSnapshot.DueSoonDays)
            .Where(d => d.Kind is "Debt" or "Installment")
            .Select(d => AlertDto.Create(
                d.Kind == "Debt" ? AlertType.DebtPaymentDueSoon : AlertType.InstallmentDueSoon,
                AlertSeverity.Info,
                $"'{d.Name}' payment due in {d.DaysUntil} day(s)",
                $"{d.Amount:N2} {s.Currency} is due on {d.DueDate:yyyy-MM-dd} for '{d.Name}'.",
                d.EntityId,
                new Dictionary<string, string>
                {
                    ["name"] = d.Name,
                    ["amount"] = Num(d.Amount),
                    ["currency"] = s.Currency,
                    ["dueDate"] = Date(d.DueDate),
                    ["daysUntil"] = Int(d.DaysUntil)
                }));

    private static IEnumerable<AlertDto> InsufficientBalanceForCardPayments(FinancialSnapshot s)
    {
        var owedSoon = s.UpcomingDueDates(FinancialSnapshot.DueSoonDays)
            .Where(d => d.Kind == "CreditCard")
            .Sum(d => d.Amount);

        if (owedSoon == 0 || s.TotalInChecking.Amount >= owedSoon)
            yield break;

        var shortfall = owedSoon - s.TotalInChecking.Amount;
        var savingsCover = s.TotalInSavings.Amount >= shortfall;

        var suggestion = savingsCover
            ? $" You could move {shortfall:N2} {s.Currency} from savings to cover it."
            : " Savings would not cover the gap either.";

        yield return AlertDto.Create(
            AlertType.InsufficientBalanceForCardPayment,
            AlertSeverity.Critical,
            "Checking balance will not cover upcoming card payments",
            $"{owedSoon:N2} {s.Currency} is due within {FinancialSnapshot.DueSoonDays} days but checking holds "
                + $"{s.TotalInChecking.Amount:N2} {s.Currency}.{suggestion}",
            parameters: new Dictionary<string, string>
            {
                ["owed"] = Num(owedSoon),
                ["checking"] = Num(s.TotalInChecking.Amount),
                ["shortfall"] = Num(shortfall),
                ["currency"] = s.Currency,
                ["days"] = Int(FinancialSnapshot.DueSoonDays),
                // A flag rather than a sentence: which suggestion to show is the
                // decision, and the wording of it belongs to whoever renders.
                ["savingsCover"] = savingsCover ? "true" : "false"
            });
    }

    private static IEnumerable<AlertDto> HighDebtRatio(FinancialSnapshot s)
    {
        if (s.DebtRatio is not { } ratio || ratio <= HighDebtRatioPercentage)
            yield break;

        yield return AlertDto.Create(
            AlertType.HighDebtRatio,
            ratio > 60m ? AlertSeverity.Critical : AlertSeverity.Warning,
            $"Debt payments take {ratio:N2}% of your income",
            $"Committed payments of {s.MonthlyObligations.Amount:N2} {s.Currency} against a net income of "
                + $"{s.MonthlyNetIncome.Amount:N2} {s.Currency}. Anything above "
                + $"{HighDebtRatioPercentage:N0}% leaves little room.",
            parameters: new Dictionary<string, string>
            {
                ["ratio"] = Num(ratio),
                ["obligations"] = Num(s.MonthlyObligations.Amount),
                ["income"] = Num(s.MonthlyNetIncome.Amount),
                ["currency"] = s.Currency,
                ["threshold"] = Num(HighDebtRatioPercentage)
            });
    }

    private static IEnumerable<AlertDto> Overspending(FinancialSnapshot s)
    {
        if (s.MonthlyNetIncome.IsZero || s.MonthSpending.Amount <= s.MonthlyNetIncome.Amount)
            yield break;

        yield return AlertDto.Create(
            AlertType.OverspendingVsIncome,
            AlertSeverity.Warning,
            "Spending exceeds income this month",
            $"You have spent {s.MonthSpending.Amount:N2} {s.Currency} this month against a net income of "
                + $"{s.MonthlyNetIncome.Amount:N2} {s.Currency}.",
            parameters: new Dictionary<string, string>
            {
                ["spent"] = Num(s.MonthSpending.Amount),
                ["income"] = Num(s.MonthlyNetIncome.Amount),
                ["currency"] = s.Currency
            });
    }

    private static IEnumerable<AlertDto> GoalAlerts(FinancialSnapshot s)
    {
        foreach (var goal in s.SavingsGoals)
        {
            var alert = GoalAlert(goal.Status, goal.Name, goal.Id, goal.ProgressPercentage);
            if (alert is not null)
                yield return alert;
        }

        foreach (var goal in s.ProductGoals)
        {
            var alert = GoalAlert(goal.Status, goal.Name, goal.Id, goal.ProgressPercentage);
            if (alert is not null)
                yield return alert;
        }
    }

    private static AlertDto? GoalAlert(GoalStatus status, string name, Guid id, decimal progress)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"] = name,
            ["progress"] = Num(progress)
        };

        return status switch
        {
            GoalStatus.BehindSchedule => AlertDto.Create(
                AlertType.GoalBehindSchedule,
                AlertSeverity.Warning,
                $"'{name}' is behind schedule",
                $"'{name}' is {progress:N2}% funded and trailing the pace needed to hit its deadline.",
                id,
                parameters),

            // Its own type: it used to share GoalBehindSchedule, which left the two
            // indistinguishable to anything rendering from the type alone.
            GoalStatus.DeadlinePassed => AlertDto.Create(
                AlertType.GoalDeadlinePassed,
                AlertSeverity.Critical,
                $"'{name}' missed its deadline",
                $"'{name}' reached its deadline at {progress:N2}% funded. Set a new deadline or adjust the target.",
                id,
                parameters),

            GoalStatus.Completed => AlertDto.Create(
                AlertType.GoalReached,
                AlertSeverity.Info,
                $"'{name}' is fully funded",
                $"You have reached the target for '{name}'.",
                id,
                parameters),

            _ => null
        };
    }
}
