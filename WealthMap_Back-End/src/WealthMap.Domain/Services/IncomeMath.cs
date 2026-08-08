using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Services;

/// <summary>
/// Normalizes incomes of different frequencies to a monthly figure, and answers
/// "how many days until the next occurrence of day-of-month N".
/// </summary>
public static class IncomeMath
{
    public static Money ToMonthly(Money amount, IncomeFrequency frequency)
    {
        var monthly = frequency switch
        {
            IncomeFrequency.Weekly => amount.Amount * 52m / 12m,
            IncomeFrequency.Biweekly => amount.Amount * 26m / 12m,
            IncomeFrequency.Yearly => amount.Amount / 12m,
            _ => amount.Amount
        };

        return new Money(monthly, amount.Currency);
    }

    public static DateOnly NextOccurrence(DateOnly today, int dayOfMonth) =>
        PaymentSchedule.NextPaymentDates(today, [dayOfMonth], 1).Single();

    public static int DaysUntil(DateOnly today, int dayOfMonth) =>
        NextOccurrence(today, dayOfMonth).DayNumber - today.DayNumber;
}
