namespace WealthMap.Application.Common.Interfaces;

/// <summary>
/// What day it is where the user is.
/// </summary>
/// <remarks>
/// Everything is stored in UTC, which is right, and for a long time everything
/// was also *computed* in UTC, which was not. The two disagree for part of every
/// day, and the error is not the day-out it looks like: at seven in the evening
/// in Guatemala, UTC has already rolled over, so "the next time the 20th comes
/// around" answers next month on the very day a card is due. A user could be told
/// a payment was thirty days away on the day it had to be made.
///
/// The zone comes from an <c>X-Time-Zone</c> header the client sends on every
/// request. A missing or unrecognised one falls back to UTC — the old behaviour,
/// which is wrong at the edges but never fails.
/// </remarks>
public interface IUserClock
{
    /// <summary>The caller's zone, or UTC when they did not say.</summary>
    TimeZoneInfo Zone { get; }

    /// <summary>Today's date where the caller is. The one to reason about.</summary>
    DateOnly Today { get; }

    /// <summary>
    /// The current instant. Identical everywhere — offered here so a handler needs
    /// one clock rather than this and <see cref="DateTime.UtcNow"/> side by side.
    /// </summary>
    DateTime UtcNow { get; }
}
