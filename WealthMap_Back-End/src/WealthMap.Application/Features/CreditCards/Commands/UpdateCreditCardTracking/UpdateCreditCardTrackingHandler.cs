using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CreditCards.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardTracking;

public class UpdateCreditCardTrackingHandler
    : ICommandHandler<UpdateCreditCardTrackingCommand, CreditCardDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CardStatementLoader _statements;

    public UpdateCreditCardTrackingHandler(
        ICreditCardRepository cards, IUnitOfWork unitOfWork, CardStatementLoader statements)
    {
        _cards = cards;
        _unitOfWork = unitOfWork;
        _statements = statements;
    }

    public async Task<CreditCardDto> Handle(
        UpdateCreditCardTrackingCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.Id);

        var mode = (TrackingMode)request.TrackingMode;

        // Order matters both ways, so the safe move is made first each time.
        // Turning sync ON: the digits must land before the mode, or the mode change
        // is refused. Turning sync OFF: the mode must drop before the digits are
        // cleared, or clearing them is refused. Doing it in one order for both cases
        // would reject one of them.
        if (mode == TrackingMode.Manual)
        {
            card.SetTrackingMode(mode);
            card.SetLastFour(request.LastFour);
        }
        else
        {
            card.SetLastFour(request.LastFour);
            card.SetTrackingMode(mode);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return await _statements.ToDtoAsync(card, request.UserId, ct);
    }
}
