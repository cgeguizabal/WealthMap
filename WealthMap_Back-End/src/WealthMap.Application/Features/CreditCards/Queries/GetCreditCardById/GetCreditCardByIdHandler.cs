using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Queries.GetCreditCardById;

public class GetCreditCardByIdHandler : IQueryHandler<GetCreditCardByIdQuery, CreditCardDto>
{
    private readonly ICreditCardRepository _cards;

    public GetCreditCardByIdHandler(ICreditCardRepository cards) => _cards = cards;

    public async Task<CreditCardDto> Handle(GetCreditCardByIdQuery request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.Id);

        return CreditCardDto.FromEntity(card);
    }
}