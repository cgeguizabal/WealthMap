using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class StoreRepository : Repository<Store>, IStoreRepository
{
    public StoreRepository(WealthMapDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default) =>
        await Set.OrderBy(s => s.Name)
                 .AsNoTracking()
                 .ToListAsync(ct);
}