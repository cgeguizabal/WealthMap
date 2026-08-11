using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class SalaryDepositRepository : Repository<SalaryDeposit>, ISalaryDepositRepository
{
    public SalaryDepositRepository(WealthMapDbContext context) : base(context) { }

    public async Task<IReadOnlyList<DateOnly>> GetPostedDatesAsync(
        Guid jobId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await Set.AsNoTracking()
                 .Where(d => d.JobId == jobId && d.ScheduledDate >= from && d.ScheduledDate <= to)
                 .Select(d => d.ScheduledDate)
                 .ToListAsync(ct);

    public async Task<IReadOnlyList<SalaryDeposit>> GetForJobAsync(
        Guid jobId, Guid userId, CancellationToken ct = default) =>
        await Set.Where(d => d.JobId == jobId && d.UserId == userId)
                 .OrderByDescending(d => d.ScheduledDate)
                 .AsNoTracking()
                 .ToListAsync(ct);
}
