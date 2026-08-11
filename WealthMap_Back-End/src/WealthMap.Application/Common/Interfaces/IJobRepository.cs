using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IJobRepository : IRepository<Job>
{
    Task<Job?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Job>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> AnyForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Every job in the system, for the automatic salary poster only.
    /// </summary>
    /// <remarks>
    /// Deliberately not user-scoped, and the single exception to that rule: the
    /// poster runs on a timer with no signed-in user, and has to pay everybody.
    /// Never call this from a request handler — those must stay user-scoped.
    /// </remarks>
    Task<IReadOnlyList<Job>> GetAllForPostingAsync(CancellationToken ct = default);
}