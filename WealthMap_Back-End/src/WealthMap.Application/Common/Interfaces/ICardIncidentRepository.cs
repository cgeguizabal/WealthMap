using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Common.Interfaces;

public interface ICardIncidentRepository : IRepository<CardIncident>
{
    Task<CardIncident?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// The report still open on one card, if there is one.
    /// </summary>
    /// <remarks>
    /// Used to close a report without making the caller name it. The user reports a
    /// card lost and later records the replacement; asking them which report that
    /// replacement settles would be asking about bookkeeping they never saw.
    /// </remarks>
    Task<CardIncident?> GetOpenForCardAsync(
        Guid userId, CardKind kind, Guid cardId, CancellationToken ct = default);

    /// <summary>Every report for one card, newest first.</summary>
    Task<IReadOnlyList<CardIncident>> GetForCardAsync(
        Guid userId, CardKind kind, Guid cardId, CancellationToken ct = default);

}
