namespace WealthMap.Domain.Enums;

/// <summary>
/// Which of the two kinds of card an incident is about.
/// </summary>
/// <remarks>
/// A discriminator rather than two tables, following <see cref="PaymentTargetType"/>:
/// the story of a lost card is identical whichever kind it was, and splitting it in
/// two would mean maintaining the same lifecycle twice.
///
/// The credit card is an entity of its own; a debit card is a pair of fields on an
/// account. So <c>CardId</c> means the card for one and the account for the other,
/// which is why the kind has to travel with it.
/// </remarks>
public enum CardKind
{
    CreditCard = 1,
    DebitCard = 2
}
