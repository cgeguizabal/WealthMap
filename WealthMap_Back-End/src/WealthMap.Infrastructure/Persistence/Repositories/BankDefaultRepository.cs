using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class BankDefaultRepository : Repository<BankDefault>, IBankDefaultRepository
{
    public BankDefaultRepository(WealthMapDbContext context) : base(context) { }

    public async Task<IReadOnlyList<BankDefault>> GetAllForUserAsync(
        Guid userId, CancellationToken ct = default) =>
        await Set.Where(b => b.UserId == userId)
            .OrderBy(b => b.BankName)
            .ThenBy(b => b.Direction)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<BankDefault?> GetByIdForUserAsync(
        Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct);

    public async Task<BankDefault?> GetForBankAsync(
        Guid userId, string bankName, TransferDirection direction, CancellationToken ct = default)
    {
        // Matched case-insensitively: the unique index is on the stored text, so
        // "BAC" and "bac" would otherwise both insert and then collide on the second
        // save rather than updating the first.
        var normalized = bankName.Trim();

        return await Set.FirstOrDefaultAsync(
            b => b.UserId == userId
                 && EF.Functions.ILike(b.BankName, normalized)
                 && b.Direction == direction,
            ct);
    }
}
