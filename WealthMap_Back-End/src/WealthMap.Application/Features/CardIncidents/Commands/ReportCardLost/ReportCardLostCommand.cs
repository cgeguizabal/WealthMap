using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.CardIncidents.Commands.ReportCardLost;

/// <param name="ReportedOn">
/// When it happened, in the user's own dates. Null means today — a report filed the
/// moment the card went missing, which is the common case.
/// </param>
public record ReportCardLostCommand(
    Guid UserId,
    CardKind Kind,
    Guid CardId,
    CardLossReason Reason,
    DateOnly? ReportedOn,
    string? Notes) : ICommand<CardIncidentDto>;
