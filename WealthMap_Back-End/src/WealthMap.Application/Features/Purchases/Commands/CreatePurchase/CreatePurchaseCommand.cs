using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Commands.CreatePurchase;

public record CreatePurchaseCommand(
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
