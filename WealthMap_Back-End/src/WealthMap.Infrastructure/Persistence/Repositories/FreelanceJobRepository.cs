using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class FreelanceJobRepository : Repository<FreelanceJob>, IFreelanceJobRepository
{
    public FreelanceJobRepository(WealthMapDbContext context) : base(context) { }

    public async Task<FreelanceJob?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId, ct);

    /// <summary>
    /// Unfinished work first, then the most recent, so the list opens on what
    /// still needs doing rather than on last year's invoices.
    /// </summary>
    /// <remarks>
    /// Sorted in memory. `Status` is computed rather than a column, so the
    /// database cannot order by it, and `title` is encrypted, so it could not
    /// sort that either.
    /// </remarks>
    public async Task<IReadOnlyList<FreelanceJob>> GetAllForUserAsync(
        Guid userId, CancellationToken ct = default)
    {
        var jobs = await Set.Where(f => f.UserId == userId).AsNoTracking().ToListAsync(ct);

        return jobs
            .OrderBy(f => f.Status switch
            {
                FreelanceJobStatus.Delivered => 0,   // waiting on money: most urgent
                FreelanceJobStatus.InProgress => 1,
                FreelanceJobStatus.Paid => 2,
                _ => 3                               // cancelled, last
            })
            .ThenByDescending(f => f.PaidOn ?? f.DeliveredOn ?? f.DueOn ?? DateOnly.MinValue)
            .ThenBy(f => f.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
