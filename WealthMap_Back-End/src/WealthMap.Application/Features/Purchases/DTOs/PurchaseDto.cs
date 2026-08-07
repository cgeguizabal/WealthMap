using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Purchases.DTOs;

public record PurchaseDto(
    Guid Id,
    string ProductName,
    decimal Amount,
    string Currency,
    DateTime OccurredAt,
    Guid? StoreId,
    string Category,
    string PaymentMethod,
    Guid? AccountId,
    Guid? CreditCardId,
    string? Notes,
    DateTime CreatedAt)
{
    public static PurchaseDto FromEntity(Purchase purchase) => new(
        purchase.Id,
        purchase.ProductName,
        purchase.Amount.Amount,
        purchase.Amount.Currency,
        purchase.OccurredAt,
        purchase.StoreId,
        purchase.Category,
        purchase.PaymentMethod.ToString(),
        purchase.AccountId,
        purchase.CreditCardId,
        purchase.Notes,
        purchase.CreatedAt);
}
