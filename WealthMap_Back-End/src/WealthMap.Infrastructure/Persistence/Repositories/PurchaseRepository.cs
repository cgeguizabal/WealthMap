using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class PurchaseRepository : Repository<Purchase>, IPurchaseRepository
{
    public PurchaseRepository(WealthMapDbContext context) : base(context) { }

    public async Task<Purchase?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, ct);

    public async Task<IReadOnlyList<Purchase>> GetPagedForUserAsync(
        Guid userId, int? year, int? month, string? category,
        int page, int pageSize, CancellationToken ct = default) =>
        await Filter(userId, year, month, category)
            .OrderByDescending(p => p.OccurredAt)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<int> CountForUserAsync(
        Guid userId, int? year, int? month, string? category, CancellationToken ct = default) =>
        await Filter(userId, year, month, category).CountAsync(ct);

    private IQueryable<Purchase> Filter(Guid userId, int? year, int? month, string? category)
    {
        var query = Set.Where(p => p.UserId == userId);

        if (year is not null)
            query = query.Where(p => p.OccurredAt.Year == year);

        if (month is not null)
            query = query.Where(p => p.OccurredAt.Month == month);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.ToLower() == category.ToLower());

        return query;
    }
}
