using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Queries.GetCreditCards;

public class GetCreditCardsHandler : IQueryHandler<GetCreditCardsQuery, IReadOnlyList<CreditCardDto>>
{
    private readonly ICreditCardRepository _cards;

    public GetCreditCardsHandler(ICreditCardRepository cards) => _cards = cards;

    public async Task<IReadOnlyList<CreditCardDto>> Handle(GetCreditCardsQuery request, CancellationToken ct)
    {
        var cards = await _cards.GetAllForUserAsync(request.UserId, ct);
        return cards.Select(CreditCardDto.FromEntity).ToList();
    }
}