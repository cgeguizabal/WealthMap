using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
}