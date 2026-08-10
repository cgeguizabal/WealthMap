using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Payments.DTOs;

public record PaymentDto(
    Guid Id,
    string TargetType,
    Guid TargetId,
    decimal Amount,
    string Currency,
    string SourceType,
    Guid? SourceAccountId,
    DateTime OccurredAt,
    string? Notes)
{
    public static PaymentDto FromEntity(Payment payment) => new(
        payment.Id,
        payment.TargetType.ToString(),
        payment.TargetId,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.SourceType.ToString(),
        payment.SourceAccountId,
        payment.OccurredAt,
        payment.Notes);
}