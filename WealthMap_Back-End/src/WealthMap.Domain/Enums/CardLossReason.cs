namespace WealthMap.Domain.Enums;

/// <summary>
/// Why a card had to be replaced.
/// </summary>
/// <remarks>
/// Kept as the user's own words about what happened rather than as anything the
/// app reasons over: nothing computes differently for a stolen card than for a
/// damaged one. It is here because six months later "why did this number change?"
/// is a question only the record can answer.
/// </remarks>
public enum CardLossReason
{
    /// <summary>Mislaid. It may still turn up, which is why recovery is a real outcome.</summary>
    Lost = 1,

    Stolen = 2,

    /// <summary>Snapped, demagnetised, chewed. The number is known, the card is not usable.</summary>
    Damaged = 3,

    /// <summary>
    /// The card is in hand but its number is not safe — skimmed, or exposed in a
    /// breach. The bank reissues for the same reason it would after a theft.
    /// </summary>
    Compromised = 4
}
