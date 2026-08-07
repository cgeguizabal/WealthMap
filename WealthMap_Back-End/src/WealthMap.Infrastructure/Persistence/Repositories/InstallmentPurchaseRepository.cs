using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class InstallmentPurchaseRepository : Repository<InstallmentPurchase>, IInstallmentPurchaseRepository
{
    public InstallmentPurchaseRepository(WealthMapDbContext context) : base(context) { }

    public async Task<InstallmentPurchase?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.Include(p => p.Payments)
                 .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, ct);

    public async Task<IReadOnlyList<InstallmentPurchase>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Include(p => p.Payments)
                 .Where(p => p.UserId == userId)
                 .OrderByDescending(p => p.PurchasedAt)
                 .AsNoTracking()
                 .ToListAsync(ct);
}
