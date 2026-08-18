using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.InstallmentPurchases.DTOs;

/// <param name="CreditCardName">
/// The card this plan was bought on. Null only when that card no longer exists.
/// </param>
/// <param name="DueThisStatement">
/// What this plan adds to the card's current statement — the installments falling
/// due on or before <paramref name="StatementDueDate"/>. Zero once they are paid.
/// </param>
/// <param name="StatementDueDate">
/// The card's next payment date, so the figure above has a deadline attached.
/// </param>
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
    string? CreditCardName,
    string? CreditCardBankName,
    decimal DueThisStatement,
    DateOnly? StatementDueDate,
    decimal RemainingBalance,
    int RemainingMonths,
    DateOnly EndDate,
    bool IsCompleted,
    IReadOnlyList<InstallmentPaymentDto> Payments,
    DateTime CreatedAt)
{
    /// <summary>
    /// Requires the card's details rather than looking them up, because a plan
    /// entity knows only its card's id. <c>InstallmentContextLoader</c> supplies
    /// them so every handler returns the same figures.
    /// </summary>
    public static InstallmentPurchaseDto FromEntity(
        InstallmentPurchase purchase,
        string? cardName,
        string? cardBankName,
        decimal dueThisStatement,
        DateOnly? statementDueDate) => new(
        purchase.Id,
        purchase.ProductName,
        purchase.TotalPrice.Amount,
        purchase.TotalPrice.Currency,
        purchase.MonthlyPayment.Amount,
        purchase.MonthsCount,
        purchase.PurchasedAt,
        purchase.StoreId,
        purchase.CreditCardId,
        cardName,
        cardBankName,
        dueThisStatement,
        statementDueDate,
        purchase.RemainingBalance.Amount,
        purchase.RemainingMonths,
        purchase.EndDate,
        purchase.IsCompleted,
        purchase.Payments.OrderBy(p => p.Number).Select(InstallmentPaymentDto.FromEntity).ToList(),
        purchase.CreatedAt);
}
