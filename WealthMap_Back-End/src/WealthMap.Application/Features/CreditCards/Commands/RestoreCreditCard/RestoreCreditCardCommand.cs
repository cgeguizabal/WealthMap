using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.CreditCards.Commands.RestoreCreditCard;

/// <summary>Brings an archived creditCard back into the lists and the totals.</summary>
public record RestoreCreditCardCommand(Guid Id, Guid UserId) : ICommand<bool>;
