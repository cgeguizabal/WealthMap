using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Jobs.DTOs;

public record SalaryDepositDto(
    Guid Id,
    Guid JobId,
    Guid AccountId,
    DateOnly ScheduledDate,
    decimal Amount,
    string Currency,
    DateTime PostedAt,
    Guid AccountMovementId)
{
    public static SalaryDepositDto FromEntity(SalaryDeposit deposit) => new(
        deposit.Id,
        deposit.JobId,
        deposit.AccountId,
        deposit.ScheduledDate,
        deposit.Amount.Amount,
        deposit.Amount.Currency,
        deposit.PostedAt,
        deposit.AccountMovementId);
}
