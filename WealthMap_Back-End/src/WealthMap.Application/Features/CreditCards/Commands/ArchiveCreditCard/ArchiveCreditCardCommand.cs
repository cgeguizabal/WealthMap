using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.CreditCards.Commands.ArchiveCreditCard;

public record ArchiveCreditCardCommand(Guid Id, Guid UserId) : ICommand<bool>;
