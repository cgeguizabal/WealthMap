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
}