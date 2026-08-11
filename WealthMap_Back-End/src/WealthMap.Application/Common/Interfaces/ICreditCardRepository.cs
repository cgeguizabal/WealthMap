using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface ICreditCardRepository : IRepository<CreditCard>
{
    Task<CreditCard?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    /// <summary>Archived cards are excluded by default; the monthly report includes them.</summary>
    Task<IReadOnlyList<CreditCard>> GetAllForUserAsync(
        Guid userId, bool includeArchived = false, CancellationToken ct = default);
}
