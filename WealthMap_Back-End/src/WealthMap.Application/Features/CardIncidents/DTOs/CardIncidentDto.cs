using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.CardIncidents.DTOs;

/// <param name="CardName">
/// What the card is called, resolved by the handler. The incident does not store it:
/// a card renamed after the report should show under its current name, and a copy
/// taken at report time would drift.
/// </param>
/// <param name="LastFourAtReport">The number that was lost, kept after the card moved on.</param>
/// <param name="NewLastFour">The replacement's number, when it differed from the old one.</param>
public record CardIncidentDto(
    Guid Id,
    string Kind,
    Guid CardId,
    string CardName,
    string Reason,
    string Status,
    DateOnly ReportedOn,
    string? LastFourAtReport,
    DateOnly? ReplacedOn,
    string? NewLastFour,
    DateOnly? RecoveredOn,
    string? Notes,
    DateTime CreatedAt)
{
    public static CardIncidentDto FromEntity(CardIncident incident, string cardName) =>
        new(
            incident.Id,
            incident.Kind.ToString(),
            incident.CardId,
            cardName,
            incident.Reason.ToString(),
            incident.Status.ToString(),
            incident.ReportedOn,
            incident.LastFourAtReport,
            incident.ReplacedOn,
            incident.NewLastFour,
            incident.RecoveredOn,
            incident.Notes,
            incident.CreatedAt);
}
