using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class AdditionalIncomeRepository : Repository<AdditionalIncome>, IAdditionalIncomeRepository
{
    public AdditionalIncomeRepository(WealthMapDbContext context) : base(context) { }

    public async Task<AdditionalIncome?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, ct);

    public async Task<IReadOnlyList<AdditionalIncome>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(i => i.UserId == userId)
                 .OrderBy(i => i.Name)
                 .AsNoTracking()
                 .ToListAsync(ct);
}