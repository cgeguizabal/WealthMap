namespace WealthMap.Domain.Enums;

/// <summary>
/// Whether an account has a debit card attached, and what kind.
/// </summary>
/// <remarks>
/// <see cref="None"/> is a member rather than a null type, because "this account
/// has no card" is an answer the user gave, not a field they skipped. Modelling it
/// as null would make those two indistinguishable.
/// </remarks>
public enum DebitCardType
{
    /// <summary>No card — the account is reached by transfer only.</summary>
    None = 1,

    /// <summary>A plastic card.</summary>
    Physical = 2,

    /// <summary>Card details issued without plastic, for online use or a wallet.</summary>
    Digital = 3
}
