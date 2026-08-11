using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Archived accounts are excluded by default. The monthly report passes
    /// <c>includeArchived: true</c> so a period that predates the archiving still
    /// reports the account that was live at the time.
    /// </summary>
    Task<IReadOnlyList<Account>> GetAllForUserAsync(
        Guid userId, bool includeArchived = false, CancellationToken ct = default);

    /// <summary>
    /// True only for an account the user can still transact with — archived ones
    /// are excluded, so nothing new can be pointed at them. Reads that need to
    /// reach archived history use <see cref="GetByIdForUserAsync"/> instead.
    /// </summary>
    Task<bool> ExistsForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
}
