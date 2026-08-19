using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.CardIncidents.Commands.ReplaceCard;

/// <param name="NewLastFour">
/// The replacement's digits, or null when the bank reissued the same number. Null
/// leaves the recorded number alone; it is not a way to clear it.
/// </param>
/// <param name="ReplacedOn">When the new card arrived. Null means today.</param>
public record ReplaceCardCommand(
    Guid UserId,
    CardKind Kind,
    Guid CardId,
    string? NewLastFour,
    DateOnly? ReplacedOn,
    string? Notes) : ICommand<CardIncidentDto>;
