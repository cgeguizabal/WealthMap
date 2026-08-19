using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Common.Services;

/// <summary>
/// A card that can be reported lost, whichever kind it is.
/// </summary>
/// <remarks>
/// A credit card is an entity; a debit card is two fields on an account. The
/// lifecycle is identical either way — reported, then replaced or found — so the
/// three handlers that drive it are written once against this and the difference is
/// resolved in one place instead of in each of them.
/// </remarks>
public interface ICardTarget
{
    /// <summary>What to call the card when reporting on it. The card's own name,
    /// or the account's for a debit card, which has no name of its own.</summary>
    string Name { get; }

    string? LastFour { get; }

    bool IsBlocked { get; }

    void ReportLost(CardLossReason reason, DateOnly reportedOn);

    void CompleteReplacement(string? newLastFour);

    void MarkRecovered();
}

public sealed class CardTargetLoader
{
    private readonly ICreditCardRepository _cards;
    private readonly IAccountRepository _accounts;

    public CardTargetLoader(ICreditCardRepository cards, IAccountRepository accounts)
    {
        _cards = cards;
        _accounts = accounts;
    }

    /// <summary>
    /// Loads the card for writing, scoped to its owner.
    /// </summary>
    /// <exception cref="NotFoundException">
    /// When it does not exist, or belongs to someone else. Both answer the same way
    /// — "not yours" is a 404 here as everywhere.
    /// </exception>
    public async Task<ICardTarget> LoadAsync(
        Guid userId, CardKind kind, Guid cardId, CancellationToken ct = default)
    {
        if (kind == CardKind.CreditCard)
        {
            var card = await _cards.GetByIdForUserAsync(cardId, userId, ct)
                ?? throw new NotFoundException("CreditCard", cardId);

            return new CreditCardTarget(card);
        }

        var account = await _accounts.GetByIdForUserAsync(cardId, userId, ct)
            ?? throw new NotFoundException("Account", cardId);

        return new DebitCardTarget(account);
    }

    /// <summary>
    /// The card's name for display, or a placeholder when it has since been deleted.
    /// </summary>
    /// <remarks>
    /// Reports outlive nothing in practice — cards are archived rather than deleted,
    /// and archiving keeps the row. The fallback is here because a report that
    /// cannot name its card should still be readable rather than fail the whole list.
    /// </remarks>
    public async Task<string> NameOfAsync(
        Guid userId, CardKind kind, Guid cardId, CancellationToken ct = default)
    {
        if (kind == CardKind.CreditCard)
        {
            var card = await _cards.GetByIdForUserAsync(cardId, userId, ct);
            return card?.CardName ?? "Deleted card";
        }

        var account = await _accounts.GetByIdForUserAsync(cardId, userId, ct);
        return account?.Name ?? "Deleted account";
    }

    private sealed class CreditCardTarget : ICardTarget
    {
        private readonly CreditCard _card;

        public CreditCardTarget(CreditCard card) => _card = card;

        public string Name => _card.CardName;
        public string? LastFour => _card.LastFour;
        public bool IsBlocked => _card.IsBlocked;

        public void ReportLost(CardLossReason reason, DateOnly reportedOn) =>
            _card.ReportLost(reason, reportedOn);

        public void CompleteReplacement(string? newLastFour) =>
            _card.CompleteReplacement(newLastFour);

        public void MarkRecovered() => _card.MarkRecovered();
    }

    private sealed class DebitCardTarget : ICardTarget
    {
        private readonly Account _account;

        public DebitCardTarget(Account account) => _account = account;

        public string Name => _account.Name;
        public string? LastFour => _account.DebitCardLastFour;
        public bool IsBlocked => _account.IsDebitCardBlocked;

        public void ReportLost(CardLossReason reason, DateOnly reportedOn) =>
            _account.ReportDebitCardLost(reason, reportedOn);

        public void CompleteReplacement(string? newLastFour) =>
            _account.CompleteDebitCardReplacement(newLastFour);

        public void MarkRecovered() => _account.MarkDebitCardRecovered();
    }
}
