using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.InstallmentPurchases.DTOs;

public record InstallmentPaymentDto(
    Guid Id,
    int Number,
    decimal Amount,
    string Currency,
    DateOnly DueDate,
    bool IsPaid,
    DateTime? PaidAt)
{
    public static InstallmentPaymentDto FromEntity(InstallmentPayment payment) => new(
        payment.Id,
        payment.Number,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.DueDate,
        payment.IsPaid,
        payment.PaidAt);
}
