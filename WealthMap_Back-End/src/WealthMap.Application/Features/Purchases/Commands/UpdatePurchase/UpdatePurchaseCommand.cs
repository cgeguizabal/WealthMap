using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Commands.UpdatePurchase;

/// <summary>
/// Same shape as creating one. A correction can change anything, including which
/// card or account paid, so nothing is withheld.
/// </summary>
public record UpdatePurchaseCommand(
    Guid Id,
    Guid UserId,
    string ProductName,
    decimal Amount,
    string? Currency,
    DateTime? OccurredAt,
    Guid? StoreId,
    string Category,
    int PaymentMethod,
    Guid? AccountId,
    Guid? CreditCardId,
    string? Notes) : ICommand<PurchaseDto>;
