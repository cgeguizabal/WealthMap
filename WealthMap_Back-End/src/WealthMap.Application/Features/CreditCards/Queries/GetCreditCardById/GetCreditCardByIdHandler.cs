using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Queries.GetCreditCardById;

public class GetCreditCardByIdHandler : IQueryHandler<GetCreditCardByIdQuery, CreditCardDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly CardStatementLoader _statements;

    public GetCreditCardByIdHandler(ICreditCardRepository cards, CardStatementLoader statements)
    {
        _cards = cards;
        _statements = statements;
    }

    public async Task<CreditCardDto> Handle(GetCreditCardByIdQuery request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.Id);

        return await _statements.ToDtoAsync(card, request.UserId, ct);
    }
}
