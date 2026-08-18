using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IAccountMovementRepository : IRepository<AccountMovement>
{
    Task<IReadOnlyList<AccountMovement>> GetPagedForAccountAsync(
        Guid accountId, Guid userId, int page, int pageSize, CancellationToken ct = default);

    Task<int> CountForAccountAsync(Guid accountId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Every movement for the user at or after <paramref name="fromInclusive"/>, across all
    /// accounts. Opening balances are derived by rewinding today's balance through these.
    /// </summary>
    Task<IReadOnlyList<AccountMovement>> GetForUserFromAsync(
        Guid userId, DateTime fromInclusive, CancellationToken ct = default);

    /// <summary>
    /// The movement written for <paramref name="relatedEntityId"/>, if any. A debit
    /// purchase writes exactly one; cash and credit purchases write none.
    /// </summary>
    Task<AccountMovement?> GetByRelatedEntityAsync(
        Guid relatedEntityId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Movements on one account strictly after a moment, oldest first. Used to
    /// rebase the running balance when an earlier movement is removed.
    /// </summary>
    Task<IReadOnlyList<AccountMovement>> GetForAccountAfterAsync(
        Guid accountId, Guid userId, DateTime after, CancellationToken ct = default);
}