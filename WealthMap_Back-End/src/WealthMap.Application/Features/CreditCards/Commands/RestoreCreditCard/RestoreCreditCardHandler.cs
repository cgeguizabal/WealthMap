using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.CreditCards.Commands.RestoreCreditCard;

/// <summary>Undoes an archive, so a mis-click is not permanent.</summary>
public class RestoreCreditCardHandler : ICommandHandler<RestoreCreditCardCommand, bool>
{
    private readonly ICreditCardRepository _cards;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreCreditCardHandler(ICreditCardRepository cards, IUnitOfWork unitOfWork)
    {
        _cards = cards;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RestoreCreditCardCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.Id);

        card.Restore();
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
