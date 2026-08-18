using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Services;

namespace WealthMap.Application.Features.Debts.DTOs;

public record DebtDto(
    Guid Id,
    string Name,
    decimal OriginalAmount,
    decimal RemainingAmount,
    string Currency,
    decimal MonthlyPayment,
    int MonthlyDueDay,
    DateOnly? NextDueDate,
    string Status,
    DateTime CreatedAt)
{
    /// <param name="today">
    /// The caller's own date, from IUserClock — "the next time the due day comes
    /// around" is a month wrong if computed from a UTC date that has rolled over.
    /// </param>
    public static DebtDto FromEntity(Debt debt, DateOnly today) => new(
        debt.Id,
        debt.Name,
        debt.OriginalAmount.Amount,
        debt.RemainingAmount.Amount,
        debt.OriginalAmount.Currency,
        debt.MonthlyPayment.Amount,
        debt.MonthlyDueDay,
        debt.Status == DebtStatus.PaidOff
            ? null
            : PaymentSchedule.NextPaymentDates(
                today, [debt.MonthlyDueDay], 1).FirstOrDefault(),
        debt.Status.ToString(),
        debt.CreatedAt);
}
