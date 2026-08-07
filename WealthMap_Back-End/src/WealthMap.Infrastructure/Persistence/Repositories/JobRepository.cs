using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class JobRepository : Repository<Job>, IJobRepository
{
    public JobRepository(WealthMapDbContext context) : base(context) { }

    public async Task<Job?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.Include(j => j.PaymentDays)
                 .Include(j => j.Deductions)
                 .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId, ct);

    public async Task<IReadOnlyList<Job>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Include(j => j.PaymentDays)
                 .Include(j => j.Deductions)
                 .Where(j => j.UserId == userId)
                 .OrderBy(j => j.Title)
                 .AsNoTracking()
                 .ToListAsync(ct);

    public async Task<bool> AnyForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.AnyAsync(j => j.UserId == userId, ct);
}