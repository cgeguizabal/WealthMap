using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Queries.GetCreditCardById;

public record GetCreditCardByIdQuery(Guid Id, Guid UserId) : IQuery<CreditCardDto>;