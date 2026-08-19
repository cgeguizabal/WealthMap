using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class CardIncidentRepository : Repository<CardIncident>, ICardIncidentRepository
{
    public CardIncidentRepository(WealthMapDbContext context) : base(context) { }

    public async Task<CardIncident?> GetByIdForUserAsync(
        Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, ct);

    /// <remarks>
    /// Open is expressed as "neither outcome date is set" rather than by the computed
    /// Status, which the database knows nothing about. There should never be more
    /// than one — the card entities refuse a second report while one is open — so
    /// this asks for the newest as a matter of determinism, not of choice.
    /// </remarks>
    public async Task<CardIncident?> GetOpenForCardAsync(
        Guid userId, CardKind kind, Guid cardId, CancellationToken ct = default) =>
        await Set
            .Where(i => i.UserId == userId
                     && i.Kind == kind
                     && i.CardId == cardId
                     && i.ReplacedOn == null
                     && i.RecoveredOn == null)
            .OrderByDescending(i => i.ReportedOn)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CardIncident>> GetForCardAsync(
        Guid userId, CardKind kind, Guid cardId, CancellationToken ct = default) =>
        await Set
            .Where(i => i.UserId == userId && i.Kind == kind && i.CardId == cardId)
            .OrderByDescending(i => i.ReportedOn)
            .ThenByDescending(i => i.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

}
