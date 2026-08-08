using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Services;

/// <summary>
/// Pure goal arithmetic shared by savings and product goals.
/// A "contribution opportunity" is a calendar month from the current one
/// through the deadline month inclusive — a deadline this month leaves exactly one.
/// </summary>
public static class GoalMath
{
    public static decimal ProgressPercentage(Money current, Money target) =>
        target.Amount == 0
            ? 0
            : decimal.Round(Math.Min(current.Amount / target.Amount, 1m) * 100m, 2);

    public static int? MonthsRemaining(DateOnly today, DateOnly? deadline)
    {
        if (deadline is null)
            return null;

        if (deadline.Value < today)
            return 0;

        return (deadline.Value.Year - today.Year) * 12 + deadline.Value.Month - today.Month + 1;
    }

    public static Money? RequiredMonthlyContribution(Money current, Money target, DateOnly today, DateOnly? deadline)
    {
        if (current >= target)
            return Money.Zero(target.Currency);

        var opportunities = MonthsRemaining(today, deadline);

        // No deadline → no required figure; deadline passed → no meaningful figure.
        if (opportunities is null or 0)
            return null;

        return new Money((target - current).Amount / opportunities.Value, target.Currency);
    }

    public static GoalStatus ComputeStatus(
        Money current, Money target, DateOnly today, DateOnly? deadline, DateOnly startedOn)
    {
        if (current >= target)
            return GoalStatus.Completed;

        if (deadline is null)
            return GoalStatus.OnTrack;

        if (deadline.Value < today)
            return GoalStatus.DeadlinePassed;

        // Linear plan from creation to deadline: behind when the saved fraction
        // trails the elapsed-time fraction.
        var totalDays = deadline.Value.DayNumber - startedOn.DayNumber;

        if (totalDays <= 0)
            return GoalStatus.OnTrack;

        var elapsedFraction = (decimal)(today.DayNumber - startedOn.DayNumber) / totalDays;
        var progressFraction = target.Amount == 0 ? 1m : current.Amount / target.Amount;

        return progressFraction >= elapsedFraction
            ? GoalStatus.OnTrack
            : GoalStatus.BehindSchedule;
    }
}
