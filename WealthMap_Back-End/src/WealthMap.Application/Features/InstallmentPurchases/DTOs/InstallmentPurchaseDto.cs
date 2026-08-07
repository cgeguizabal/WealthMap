using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.InstallmentPurchases.DTOs;

public record InstallmentPurchaseDto(
    Guid Id,
    string ProductName,
    decimal TotalPrice,
    string Currency,
    decimal MonthlyPayment,
    int MonthsCount,
    DateOnly PurchasedAt,
    Guid? StoreId,
    Guid CreditCardId,
    decimal RemainingBalance,
    int RemainingMonths,
    DateOnly EndDate,
    bool IsCompleted,
    IReadOnlyList<InstallmentPaymentDto> Payments,
    DateTime CreatedAt)
{
    public static InstallmentPurchaseDto FromEntity(InstallmentPurchase purchase) => new(
        purchase.Id,
        purchase.ProductName,
        purchase.TotalPrice.Amount,
        purchase.TotalPrice.Currency,
        purchase.MonthlyPayment.Amount,
        purchase.MonthsCount,
        purchase.PurchasedAt,
        purchase.StoreId,
        purchase.CreditCardId,
        purchase.RemainingBalance.Amount,
        purchase.RemainingMonths,
        purchase.EndDate,
        purchase.IsCompleted,
        purchase.Payments.OrderBy(p => p.Number).Select(InstallmentPaymentDto.FromEntity).ToList(),
        purchase.CreatedAt);
}
