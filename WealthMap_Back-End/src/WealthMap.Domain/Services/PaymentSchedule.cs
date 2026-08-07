using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Services;

/// <summary>
/// Pure date arithmetic for salary payment days. A payment day like "the 30th"
/// clamps to the last day of shorter months (Feb 30 → Feb 28/29).
/// </summary>
public static class PaymentSchedule
{
    public static DateOnly ClampToMonth(int year, int month, int day)
    {
        if (day is < 1 or > 31)
            throw new DomainException("Payment day must be between 1 and 31.");

        return new DateOnly(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));
    }

    public static IReadOnlyList<DateOnly> NextPaymentDates(
        DateOnly from, IEnumerable<int> paymentDays, int count)
    {
        var days = paymentDays.Distinct().ToArray();

        if (days.Length == 0 || count <= 0)
            return [];

        var result = new List<DateOnly>(count);
        var (year, month) = (from.Year, from.Month);

        while (result.Count < count)
        {
            foreach (var date in days.Select(d => ClampToMonth(year, month, d)).Order())
            {
                if (date >= from && result.Count < count)
                    result.Add(date);
            }

            (year, month) = month == 12 ? (year + 1, 1) : (year, month + 1);
        }

        return result;
    }
}