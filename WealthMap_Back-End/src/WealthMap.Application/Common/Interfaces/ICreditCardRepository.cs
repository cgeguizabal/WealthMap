using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface ICreditCardRepository : IRepository<CreditCard>
{
    Task<CreditCard?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<CreditCard>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
}