using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Queries.GetCreditCards;

/// <param name="IncludeArchived">
/// Archived cards are hidden from every list and total by default. The settings
/// screen asks for them so they can be brought back.
/// </param>
public record GetCreditCardsQuery(
    Guid UserId,
    bool IncludeArchived = false) : IQuery<IReadOnlyList<CreditCardDto>>;