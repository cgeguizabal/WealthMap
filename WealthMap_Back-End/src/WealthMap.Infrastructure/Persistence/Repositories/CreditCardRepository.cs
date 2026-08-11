using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class CreditCardRepository : Repository<CreditCard>, ICreditCardRepository
{
    public CreditCardRepository(WealthMapDbContext context) : base(context) { }

    /// <summary>Archived cards are still fetchable by id, so their detail page keeps working.</summary>
    public async Task<CreditCard?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public async Task<IReadOnlyList<CreditCard>> GetAllForUserAsync(
        Guid userId, bool includeArchived = false, CancellationToken ct = default)
    {
        var query = Set.Where(c => c.UserId == userId);

        if (!includeArchived)
            query = query.Where(c => !c.IsArchived);

        return await query
            .OrderBy(c => c.CardName)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
