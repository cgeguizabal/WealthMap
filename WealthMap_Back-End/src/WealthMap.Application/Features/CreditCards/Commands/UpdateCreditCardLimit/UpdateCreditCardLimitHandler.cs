using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CreditCards.DTOs;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardLimit;

public class UpdateCreditCardLimitHandler : ICommandHandler<UpdateCreditCardLimitCommand, CreditCardDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CardStatementLoader _statements;

    public UpdateCreditCardLimitHandler(
        ICreditCardRepository cards, IUnitOfWork unitOfWork, CardStatementLoader statements)
    {
        _cards = cards;
        _unitOfWork = unitOfWork;
        _statements = statements;
    }

    public async Task<CreditCardDto> Handle(UpdateCreditCardLimitCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.Id);

        // The card's own currency: limits can't switch currency, the entity enforces it.
        card.UpdateCreditLimit(new Money(request.NewLimit, card.CreditLimit.Currency));

        await _unitOfWork.SaveChangesAsync(ct);

        return await _statements.ToDtoAsync(card, request.UserId, ct);
    }
}