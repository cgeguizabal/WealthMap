using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IFreelanceJobRepository : IRepository<FreelanceJob>
{
    Task<FreelanceJob?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<FreelanceJob>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
}
