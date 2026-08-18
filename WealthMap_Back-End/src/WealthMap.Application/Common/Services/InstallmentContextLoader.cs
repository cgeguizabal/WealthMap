using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Common.Services;

/// <summary>
/// Builds installment DTOs carrying the card they were bought on and what they
/// add to that card's current statement.
/// </summary>
/// <remarks>
/// A plan is meaningless without its card. The card decides when its installments
/// fall due, so "what does this plan cost me this month" cannot be answered from
/// the plan alone — and a plan screen that cannot say which card was charged
/// leaves the user to go and find out.
///
/// The mirror of <see cref="CardStatementLoader"/>, and for the same reason: five
/// handlers return a plan, and figures computed independently would disagree.
/// </remarks>
public class InstallmentContextLoader
{
    private readonly ICreditCardRepository _cards;
    private readonly IUserClock _clock;

    public InstallmentContextLoader(ICreditCardRepository cards, IUserClock clock)
    {
        _cards = cards;
        _clock = clock;
    }

    public async Task<InstallmentPurchaseDto> ToDtoAsync(
        InstallmentPurchase purchase, Guid userId, CancellationToken ct)
        => (await ToDtoListAsync([purchase], userId, ct))[0];

    public async Task<IReadOnlyList<InstallmentPurchaseDto>> ToDtoListAsync(
        IReadOnlyList<InstallmentPurchase> purchases, Guid userId, CancellationToken ct)
    {
        if (purchases.Count == 0) return [];

        // includeArchived: a plan outlives the archiving of the card it sits on,
        // and a row that cannot name its card is worse than one naming an archived
        // one. The plan's debt is real either way.
        var cards = await _cards.GetAllForUserAsync(userId, includeArchived: true, ct: ct);
        var byId = cards.ToDictionary(c => c.Id);

        // The caller's date; see FinancialSnapshotLoader for why UTC is wrong here.
        var today = _clock.Today;

        return purchases
            .Select(purchase => Build(purchase, byId.GetValueOrDefault(purchase.CreditCardId), today))
            .ToList();
    }

    private static InstallmentPurchaseDto Build(
        InstallmentPurchase purchase, CreditCard? card, DateOnly today)
    {
        var zero = Money.Zero(purchase.TotalPrice.Currency);

        if (card is null)
            return InstallmentPurchaseDto.FromEntity(purchase, null, null, zero.Amount, null);

        // The same date the card's own statement split reserves against, so the
        // plan screen and the card screen name the same figure for the same month.
        var dueDate = LiquidityProjection.StatementDueDate(
            today, card.StatementCutoffDay, card.PaymentDueDay);

        var dueThisStatement = purchase.Payments
            .Where(p => !p.IsPaid && p.DueDate <= dueDate)
            .Aggregate(zero, (sum, p) => sum + p.Amount);

        return InstallmentPurchaseDto.FromEntity(
            purchase, card.CardName, card.BankName, dueThisStatement.Amount, dueDate);
    }
}
