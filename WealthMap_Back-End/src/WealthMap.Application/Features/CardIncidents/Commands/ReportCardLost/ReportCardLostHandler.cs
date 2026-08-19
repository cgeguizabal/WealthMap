using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.CardIncidents.Commands.ReportCardLost;

public class ReportCardLostHandler : ICommandHandler<ReportCardLostCommand, CardIncidentDto>
{
    private readonly CardTargetLoader _targets;
    private readonly ICardIncidentRepository _incidents;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public ReportCardLostHandler(
        CardTargetLoader targets,
        ICardIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        IUserClock clock)
    {
        _targets = targets;
        _incidents = incidents;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<CardIncidentDto> Handle(ReportCardLostCommand request, CancellationToken ct)
    {
        var card = await _targets.LoadAsync(request.UserId, request.Kind, request.CardId, ct);

        var reportedOn = request.ReportedOn ?? _clock.Today;

        // Copied off the card before it is blocked, because this is the only place
        // the lost number survives once a replacement overwrites it.
        var incident = new CardIncident(
            request.UserId,
            request.Kind,
            request.CardId,
            request.Reason,
            reportedOn,
            card.LastFour,
            request.Notes);

        // The card's own blocked date and the report are one fact recorded in two
        // places; a crash between them would leave a card out of service with
        // nothing explaining why, or a report about a card still being spent on.
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            card.ReportLost(request.Reason, reportedOn);
            await _incidents.AddAsync(incident, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, ct);

        return CardIncidentDto.FromEntity(incident, card.Name);
    }
}
