using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Purchases.Commands.DeletePurchase;

public record DeletePurchaseCommand(Guid Id, Guid UserId) : ICommand<bool>;
