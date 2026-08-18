using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Queries.GetCreditCards;

public class GetCreditCardsHandler : IQueryHandler<GetCreditCardsQuery, IReadOnlyList<CreditCardDto>>
{
    private readonly ICreditCardRepository _cards;
    private readonly CardStatementLoader _statements;

    public GetCreditCardsHandler(ICreditCardRepository cards, CardStatementLoader statements)
    {
        _cards = cards;
        _statements = statements;
    }

    public async Task<IReadOnlyList<CreditCardDto>> Handle(GetCreditCardsQuery request, CancellationToken ct)
    {
        var cards = await _cards.GetAllForUserAsync(
            request.UserId, request.IncludeArchived, ct);

        return await _statements.ToDtoListAsync(cards, request.UserId, ct);
    }
}
