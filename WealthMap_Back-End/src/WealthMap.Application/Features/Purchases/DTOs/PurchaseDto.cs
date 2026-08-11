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
    DateTime CreatedAt,
    /// <summary>Where it was bought. Null when the purchase named no store.</summary>
    string? StoreName = null)
{
    /// <summary>
    /// <paramref name="storeName"/> is passed in rather than read off the entity:
    /// stores are a shared catalogue with their own repository, and a purchase
    /// holds only the id. Callers that have not resolved it leave it null.
    /// </summary>
    public static PurchaseDto FromEntity(Purchase purchase, string? storeName = null) => new(
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
        purchase.CreatedAt,
        storeName);
}
