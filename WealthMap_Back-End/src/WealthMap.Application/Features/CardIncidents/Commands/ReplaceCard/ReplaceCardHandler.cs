using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CardIncidents.DTOs;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.CardIncidents.Commands.ReplaceCard;

/// <summary>
/// Records the card the bank sent to replace one that was reported.
/// </summary>
/// <remarks>
/// Deliberately not addressed by report id. The user reports a card lost and, weeks
/// later, a new one arrives; asking which report that settles would be asking about
/// bookkeeping they never saw. There is at most one open report per card — the card
/// itself refuses a second — so naming the card is unambiguous.
/// </remarks>
public class ReplaceCardHandler : ICommandHandler<ReplaceCardCommand, CardIncidentDto>
{
    private readonly CardTargetLoader _targets;
    private readonly ICardIncidentRepository _incidents;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public ReplaceCardHandler(
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

    public async Task<CardIncidentDto> Handle(ReplaceCardCommand request, CancellationToken ct)
    {
        var card = await _targets.LoadAsync(request.UserId, request.Kind, request.CardId, ct);

        var incident = await _incidents.GetOpenForCardAsync(
                request.UserId, request.Kind, request.CardId, ct)
            // A rule, not a missing resource: the card exists, it simply was never
            // reported. The domain says the same thing when the card is asked to
            // complete a replacement it never needed, and both answer 400.
            ?? throw new DomainException(
                "There is no open report for this card. Report it lost or stolen first.");

        var replacedOn = request.ReplacedOn ?? _clock.Today;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            incident.RecordReplacement(request.NewLastFour, replacedOn, request.Notes);

            // Order matters only for readability; both are in one transaction. The
            // card takes the new number and comes back into service, which is what
            // makes its credit spendable again.
            card.CompleteReplacement(request.NewLastFour);

            await _unitOfWork.SaveChangesAsync(ct);
        }, ct);

        return CardIncidentDto.FromEntity(incident, card.Name);
    }
}
