using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Features.CreditCards.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Common.Services;

/// <summary>
/// Builds credit card DTOs with the statement split filled in.
/// </summary>
/// <remarks>
/// Exists so the split is computed in one place. Seven handlers return a card, and
/// if each loaded its own history they would drift — one screen would say $50 is
/// due and another $100 for the same card, which is worse than not showing the
/// figure at all.
/// </remarks>
public class CardStatementLoader
{
    private readonly IPurchaseRepository _purchases;
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly IUserClock _clock;

    public CardStatementLoader(
        IPurchaseRepository purchases,
        IInstallmentPurchaseRepository installments,
        IUserClock clock)
    {
        _purchases = purchases;
        _installments = installments;
        _clock = clock;
    }

    public async Task<CreditCardDto> ToDtoAsync(CreditCard card, Guid userId, CancellationToken ct)
        => (await ToDtoListAsync([card], userId, ct))[0];

    public async Task<IReadOnlyList<CreditCardDto>> ToDtoListAsync(
        IReadOnlyList<CreditCard> cards, Guid userId, CancellationToken ct)
    {
        if (cards.Count == 0) return [];

        // The caller's date, not UTC: every cutoff and due date below is "the
        // next time this day comes around", which answers next month if the
        // date has already rolled over where the server is but not where the
        // user is.
        var today = _clock.Today;

        // The earliest cutoff across the cards, so one query covers all of them
        // however differently their cycles are set.
        var earliestCutoff = cards.Min(c => StatementCycle.LastCutoff(today, c.StatementCutoffDay));
        var since = earliestCutoff.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var purchases = await _purchases.GetCardPurchasesSinceAsync(userId, since, ct);
        var plans = await _installments.GetAllForUserAsync(userId, ct);

        return cards.Select(card => FromEntity(card, today, purchases, plans)).ToList();
    }

    private static CreditCardDto FromEntity(
        CreditCard card,
        DateOnly today,
        IReadOnlyList<Purchase> purchases,
        IReadOnlyList<InstallmentPurchase> plans)
    {
        var zero = Money.Zero(card.CreditLimit.Currency);

        var lastCutoff = StatementCycle.LastCutoff(today, card.StatementCutoffDay);
        var dueDate = LiquidityProjection.StatementDueDate(
            today, card.StatementCutoffDay, card.PaymentDueDay);

        // Filtered per card: the query fetched every card's purchases at once, and
        // each card's own cutoff decides which of them are still in the open cycle.
        var chargedSinceCutoff = purchases
            .Where(p => p.CreditCardId == card.Id
                        && DateOnly.FromDateTime(p.OccurredAt) > lastCutoff
                        && p.Amount.Currency == card.CreditLimit.Currency)
            .Aggregate(zero, (sum, p) => sum + p.Amount);

        var cardPlans = plans.Where(i => i.CreditCardId == card.Id && !i.IsCompleted).ToList();

        var installmentRemaining = cardPlans
            .Aggregate(zero, (sum, i) => sum + i.RemainingBalance);

        var installmentDueThisCycle = cardPlans
            .Select(i => i.NextUnpaid())
            .Where(i => i.DueDate <= dueDate)
            .Aggregate(zero, (sum, i) => sum + i.Amount);

        var split = StatementCycle.Split(
            today,
            card.StatementCutoffDay,
            card.UsedCredit,
            chargedSinceCutoff,
            installmentRemaining,
            installmentDueThisCycle);

        return CreditCardDto.FromEntity(card, split, today);
    }
}
