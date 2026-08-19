namespace WealthMap.Domain.Enums;

/// <summary>
/// Where a reported card has got to.
/// </summary>
/// <remarks>
/// Computed from the dates on <see cref="Entities.CardIncident"/> rather than
/// stored, for the same reason as <see cref="FreelanceJobStatus"/>: the dates are
/// the facts, and a stored status could come to disagree with them.
/// </remarks>
public enum CardIncidentStatus
{
    /// <summary>Reported, and the bank has not replaced it yet. The card cannot be used.</summary>
    Open = 1,

    /// <summary>A new card arrived, carrying a new number.</summary>
    Replaced = 2,

    /// <summary>It turned up, or the report was a false alarm. The old number still stands.</summary>
    Recovered = 3
}
