using System.Text.RegularExpressions;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Common.Validation;

/// <summary>
/// The tracking checks, shared by the four validators that need them.
/// </summary>
/// <remarks>
/// The domain enforces these too. Validating first means the caller gets a
/// field-keyed 400 naming <c>lastFour</c> rather than a generic business-rule 400,
/// which is what the form needs in order to mark the right input.
/// </remarks>
public static partial class TrackingRules
{
    public const string LastFourMessage = "Last four digits must be exactly 4 numbers.";
    public const string SyncNeedsLastFourMessage = "Last 4 digits are required to enable email sync.";
    public const string TrackingModeMessage = "Tracking mode must be 1 (Manual) or 2 (EmailSync).";

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex FourDigits();

    /// <summary>Absent is fine; present must be exactly four ASCII digits.</summary>
    public static bool IsValidLastFour(string? lastFour) =>
        string.IsNullOrWhiteSpace(lastFour) || FourDigits().IsMatch(lastFour.Trim());

    public static bool IsDefinedMode(int trackingMode) =>
        Enum.IsDefined(typeof(TrackingMode), trackingMode);

    /// <summary>
    /// The cross-field rule: opting an instrument into sync without the digits that
    /// would identify it in an email is rejected before the domain ever sees it.
    /// </summary>
    public static bool IsIdentifiable(int trackingMode, string? lastFour) =>
        trackingMode != (int)TrackingMode.EmailSync || !string.IsNullOrWhiteSpace(lastFour);
}
