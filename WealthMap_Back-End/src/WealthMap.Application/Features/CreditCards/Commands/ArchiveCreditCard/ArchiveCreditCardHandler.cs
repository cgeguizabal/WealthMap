using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.CreditCards.Commands.ArchiveCreditCard;

/// <summary>
/// Removes the card from the user's view. Purchases, installment plans and
/// payments that reference it are left intact.
/// </summary>
public class ArchiveCreditCardHandler : ICommandHandler<ArchiveCreditCardCommand, bool>
{
    private readonly ICreditCardRepository _cards;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveCreditCardHandler(ICreditCardRepository cards, IUnitOfWork unitOfWork)
    {
        _cards = cards;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ArchiveCreditCardCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.Id);

        card.Archive();
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
