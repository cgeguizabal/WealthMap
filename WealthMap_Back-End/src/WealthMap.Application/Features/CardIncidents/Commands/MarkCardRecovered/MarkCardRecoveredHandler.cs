using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.CardIncidents.Commands.MarkCardRecovered;

/// <summary>
/// Closes a report because the card came back, or was never really gone.
/// </summary>
/// <remarks>
/// The report is kept rather than deleted. Without this the only way out of a
/// mistaken report would be to record a replacement that never arrived, which would
/// put a fiction in the history to correct a mistake.
/// </remarks>
public class MarkCardRecoveredHandler : ICommandHandler<MarkCardRecoveredCommand, CardIncidentDto>
{
    private readonly CardTargetLoader _targets;
    private readonly ICardIncidentRepository _incidents;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public MarkCardRecoveredHandler(
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

    public async Task<CardIncidentDto> Handle(MarkCardRecoveredCommand request, CancellationToken ct)
    {
        var card = await _targets.LoadAsync(request.UserId, request.Kind, request.CardId, ct);

        var incident = await _incidents.GetOpenForCardAsync(
                request.UserId, request.Kind, request.CardId, ct)
            ?? throw new DomainException("There is no open report for this card.");

        var recoveredOn = request.RecoveredOn ?? _clock.Today;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            incident.RecordRecovery(recoveredOn, request.Notes);
            card.MarkRecovered();

            await _unitOfWork.SaveChangesAsync(ct);
        }, ct);

        return CardIncidentDto.FromEntity(incident, card.Name);
    }
}
