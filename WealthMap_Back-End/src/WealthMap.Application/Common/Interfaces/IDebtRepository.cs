using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IDebtRepository : IRepository<Debt>
{
    Task<Debt?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Debt>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
}
