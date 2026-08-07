using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IJobRepository : IRepository<Job>
{
    Task<Job?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Job>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> AnyForUserAsync(Guid userId, CancellationToken ct = default);
}