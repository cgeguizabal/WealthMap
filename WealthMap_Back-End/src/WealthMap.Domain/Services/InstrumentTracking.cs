using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Services;

/// <summary>
/// The rules governing an instrument's identifying digits and its tracking mode.
/// </summary>
/// <remarks>
/// Shared by <c>Account</c> and <c>CreditCard</c> because both are identified the
/// same way in a bank's notification email, and the two must fail with the same
/// wording. Duplicating the check in each entity would let the messages drift, and
/// a user reading "must be exactly 4 numbers" on one screen and something else on
/// the other would reasonably conclude the rules differ.
/// </remarks>
public static class InstrumentTracking
{
    private const int RequiredLength = 4;

    /// <summary>
    /// Normalises the digits, or returns null when cleared.
    /// </summary>
    /// <remarks>
    /// Blank and null are the same intent — "I don't want this set" — so both
    /// collapse to null rather than storing an empty string that would later have
    /// to be tested for separately everywhere.
    /// </remarks>
    public static string? NormalizeLastFour(string? lastFour)
    {
        if (string.IsNullOrWhiteSpace(lastFour)) return null;

        var trimmed = lastFour.Trim();

        // AsciiDigit rather than char.IsDigit: the latter accepts Arabic-Indic and
        // other Unicode digits, which would never match what a bank email prints.
        if (trimmed.Length != RequiredLength || !trimmed.All(char.IsAsciiDigit))
            throw new DomainException("Last four digits must be exactly 4 numbers.");

        return trimmed;
    }

    /// <summary>
    /// Guards the one invariant: an instrument can never be in <see cref="TrackingMode.EmailSync"/>
    /// without the digits that would let an email be matched to it.
    /// </summary>
    /// <remarks>
    /// Enforced on both transitions, not only when switching mode. Clearing the
    /// digits on a synced instrument would otherwise leave a row that claims to be
    /// automated but can never be identified — silently unreachable rather than
    /// loudly wrong.
    /// </remarks>
    public static void EnsureIdentifiable(TrackingMode mode, string? lastFour)
    {
        if (mode == TrackingMode.EmailSync && string.IsNullOrWhiteSpace(lastFour))
            throw new DomainException("Last 4 digits are required to enable email sync.");
    }
}
