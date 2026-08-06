using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class CreditCardRepository : Repository<CreditCard>, ICreditCardRepository
{
    public CreditCardRepository(WealthMapDbContext context) : base(context) { }

    public async Task<CreditCard?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public async Task<IReadOnlyList<CreditCard>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(c => c.UserId == userId)
                 .OrderBy(c => c.CardName)
                 .AsNoTracking()
                 .ToListAsync(ct);
}