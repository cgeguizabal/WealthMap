using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.CardIncidents.Queries.GetCardIncidents;

public class GetCardIncidentsHandler
    : IQueryHandler<GetCardIncidentsQuery, IReadOnlyList<CardIncidentDto>>
{
    private readonly ICardIncidentRepository _incidents;
    private readonly CardTargetLoader _targets;

    public GetCardIncidentsHandler(ICardIncidentRepository incidents, CardTargetLoader targets)
    {
        _incidents = incidents;
        _targets = targets;
    }

    public async Task<IReadOnlyList<CardIncidentDto>> Handle(
        GetCardIncidentsQuery request, CancellationToken ct)
    {
        var incidents = await _incidents.GetForCardAsync(
            request.UserId, request.Kind, request.CardId, ct);

        if (incidents.Count == 0) return [];

        // One lookup for the whole list: every report here is about the same card,
        // and it is named once rather than once per report.
        var name = await _targets.NameOfAsync(request.UserId, request.Kind, request.CardId, ct);

        return incidents.Select(i => CardIncidentDto.FromEntity(i, name)).ToList();
    }
}
