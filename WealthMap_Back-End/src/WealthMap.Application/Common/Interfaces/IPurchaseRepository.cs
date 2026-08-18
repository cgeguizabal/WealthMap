using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<Purchase?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Purchase>> GetPagedForUserAsync(
        Guid userId, int? year, int? month, string? category, Guid? creditCardId,
        int page, int pageSize, CancellationToken ct = default);

    Task<int> CountForUserAsync(
        Guid userId, int? year, int? month, string? category, Guid? creditCardId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Purchase>> GetForUserInMonthAsync(
        Guid userId, int year, int month, CancellationToken ct = default);

    /// <summary>
    /// Card purchases dated on or after <paramref name="since"/>, across every card.
    /// </summary>
    /// <remarks>
    /// One query for all cards rather than one per card: the cards screen renders
    /// them together, and the caller groups by <c>CreditCardId</c>. Cash and debit
    /// purchases are excluded because they never touched a card balance.
    /// </remarks>
    Task<IReadOnlyList<Purchase>> GetCardPurchasesSinceAsync(
        Guid userId, DateTime since, CancellationToken ct = default);
}
