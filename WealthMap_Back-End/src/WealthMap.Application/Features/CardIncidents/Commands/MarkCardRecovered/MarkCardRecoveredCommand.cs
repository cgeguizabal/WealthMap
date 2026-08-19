using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.CardIncidents.Commands.MarkCardRecovered;

/// <param name="RecoveredOn">When it turned up. Null means today.</param>
public record MarkCardRecoveredCommand(
    Guid UserId,
    CardKind Kind,
    Guid CardId,
    DateOnly? RecoveredOn,
    string? Notes) : ICommand<CardIncidentDto>;
