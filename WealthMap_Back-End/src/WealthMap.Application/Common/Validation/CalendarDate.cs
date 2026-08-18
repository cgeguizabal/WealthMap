namespace WealthMap.Application.Common.Validation;

/// <summary>
/// Compares a date the user picked against today, allowing for the fact that the
/// server does not know what day it is where they are.
/// </summary>
/// <remarks>
/// A <see cref="DateOnly"/> arrives as a calendar date chosen in the user's own
/// time zone. The server compares it against the UTC date, and the two disagree
/// for part of every day: at 07:00 in Tokyo it is still yesterday in UTC, and at
/// 19:00 in Guatemala it is already tomorrow.
///
/// That made three validators reject perfectly good input. Someone in Tokyo
/// recording a purchase first thing in the morning was told the date was in the
/// future; someone in Guatemala setting a goal deadline in the evening was told
/// it was in the past. Both are the app being wrong about what day it is, and
/// there is no message that makes that acceptable.
///
/// One day of slack covers every zone — UTC-12 to UTC+14 is never more than a
/// calendar day either side. The cost is that a date genuinely one day out slips
/// through, which for "when did you buy this" and "when is this goal due" is not
/// worth a single false rejection.
///
/// This is a tolerance, not a fix. The complete answer is to know the user's zone
/// and compare against their date, the way the monthly report now does. The rest
/// of the app still reasons in UTC, and the day-counting it does for display can
/// be a day out for part of each day.
/// </remarks>
public static class CalendarDate
{
    private static DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>True when the date is today or earlier, anywhere on earth.</summary>
    public static bool IsNotInTheFuture(DateOnly date) => date <= UtcToday.AddDays(1);

    /// <summary>True when the date is today or later, anywhere on earth.</summary>
    public static bool IsNotInThePast(DateOnly date) => date >= UtcToday.AddDays(-1);
}
