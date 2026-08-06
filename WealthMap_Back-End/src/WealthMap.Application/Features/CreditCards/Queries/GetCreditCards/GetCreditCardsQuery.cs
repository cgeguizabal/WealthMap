using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Queries.GetCreditCards;

public record GetCreditCardsQuery(Guid UserId) : IQuery<IReadOnlyList<CreditCardDto>>;