using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.CardIncidents.Queries.GetCardIncidents;

/// <summary>One card's history: every time it was reported, and how each ended.</summary>
public record GetCardIncidentsQuery(
    Guid UserId,
    CardKind Kind,
    Guid CardId) : IQuery<IReadOnlyList<CardIncidentDto>>;
