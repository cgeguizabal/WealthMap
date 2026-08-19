using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.Services;

namespace WealthMap.Domain.Entities;

/// <summary>
/// The record of a card going out of service: reported lost, stolen, damaged or
/// compromised, and then either replaced with a new number or recovered.
/// </summary>
/// <remarks>
/// The number on a card was always editable, so this is not about making the digits
/// changeable. It is about the change having a reason attached. A card's last four
/// silently becoming different ones is indistinguishable from a typo being fixed,
/// and the difference matters: one is a correction, the other is a card that stopped
/// existing on a particular day.
///
/// It carries its own copy of the digits at both ends — <see cref="LastFourAtReport"/>
/// and <see cref="NewLastFour"/> — because the card only ever holds its current
/// number. Without the snapshot, replacing a card would erase the very thing the
/// record exists to remember.
///
/// Status is computed from the dates, as on <see cref="FreelanceJob"/>. Both
/// outcomes are terminal and mutually exclusive: a card is either back in the
/// user's hand or superseded by a new one, never both.
/// </remarks>
public class CardIncident : BaseEntity
{
    public Guid UserId { get; private set; }

    public CardKind Kind { get; private set; }

    /// <summary>
    /// The credit card, or the account the debit card belongs to — see
    /// <see cref="CardKind"/> for why one field means two things.
    /// </summary>
    public Guid CardId { get; private set; }

    public CardLossReason Reason { get; private set; }

    /// <summary>
    /// The day the card stopped being usable, as the user reports it. Their own
    /// date, not UTC: someone reporting a theft late on the 3rd means the 3rd.
    /// </summary>
    public DateOnly ReportedOn { get; private set; }

    /// <summary>The number that was lost. Null when the card never had one recorded.</summary>
    public string? LastFourAtReport { get; private set; }

    public DateOnly? ReplacedOn { get; private set; }

    /// <summary>
    /// The number the replacement arrived with. Null when the bank reissued the same
    /// number, which some do for a damaged card.
    /// </summary>
    public string? NewLastFour { get; private set; }

    public DateOnly? RecoveredOn { get; private set; }

    public string? Notes { get; private set; }

    public CardIncidentStatus Status =>
        ReplacedOn is not null ? CardIncidentStatus.Replaced
        : RecoveredOn is not null ? CardIncidentStatus.Recovered
        : CardIncidentStatus.Open;

    public bool IsOpen => Status == CardIncidentStatus.Open;

    private CardIncident() { }

    public CardIncident(
        Guid userId,
        CardKind kind,
        Guid cardId,
        CardLossReason reason,
        DateOnly reportedOn,
        string? lastFourAtReport,
        string? notes = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Card report must belong to a user.");

        if (cardId == Guid.Empty)
            throw new DomainException("Card report must reference a card.");

        if (!Enum.IsDefined(kind))
            throw new DomainException("Card report must be about a credit card or a debit card.");

        if (!Enum.IsDefined(reason))
            throw new DomainException("Card report must say what happened to the card.");

        UserId = userId;
        Kind = kind;
        CardId = cardId;
        Reason = reason;
        ReportedOn = reportedOn;
        LastFourAtReport = InstrumentTracking.NormalizeLastFour(lastFourAtReport);
        Notes = Clean(notes);
    }

    /// <summary>
    /// Closes the report with the replacement the bank sent.
    /// </summary>
    /// <param name="newLastFour">
    /// The new number, or null when the replacement carries the old one.
    /// </param>
    public void RecordReplacement(string? newLastFour, DateOnly replacedOn, string? notes = null)
    {
        EnsureOpen("record a replacement for");

        if (replacedOn < ReportedOn)
            throw new DomainException(
                "A replacement cannot arrive before the card was reported.");

        NewLastFour = InstrumentTracking.NormalizeLastFour(newLastFour);
        ReplacedOn = replacedOn;
        Notes = Clean(notes) ?? Notes;
        Touch();
    }

    /// <summary>
    /// Closes the report because the card came back.
    /// </summary>
    /// <remarks>
    /// The report is kept rather than deleted. A card that was missing for a week
    /// is worth remembering even once it is found, and deleting the record would
    /// leave the days it was unusable unexplained.
    /// </remarks>
    public void RecordRecovery(DateOnly recoveredOn, string? notes = null)
    {
        EnsureOpen("record a recovery for");

        if (recoveredOn < ReportedOn)
            throw new DomainException("A card cannot be found before it was reported.");

        RecoveredOn = recoveredOn;
        Notes = Clean(notes) ?? Notes;
        Touch();
    }

    private void EnsureOpen(string action)
    {
        if (IsOpen) return;

        var outcome = Status == CardIncidentStatus.Replaced ? "replaced" : "recovered";

        throw new DomainException(
            $"Cannot {action} this report: the card was already {outcome}.");
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
