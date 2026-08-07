using WealthMap.Domain.Entities;
using WealthMap.Domain.Services;

namespace WealthMap.Application.Features.Jobs.DTOs;

public record JobDto(
    Guid Id,
    string Title,
    string Employer,
    decimal GrossMonthlySalary,
    string Currency,
    decimal NetMonthly,
    decimal NetPerDeposit,
    Guid DepositAccountId,
    IReadOnlyList<int> PaymentDays,
    IReadOnlyList<DeductionDto> Deductions,
    IReadOnlyList<DateOnly> NextPaymentDates,
    DateTime CreatedAt)
{
    public static JobDto FromEntity(Job job)
    {
        var paymentDays = job.PaymentDays
            .Select(d => d.DayOfMonth)
            .OrderBy(d => d)
            .ToList();

        return new JobDto(
            job.Id,
            job.Title,
            job.Employer,
            job.GrossMonthlySalary.Amount,
            job.GrossMonthlySalary.Currency,
            job.NetMonthly.Amount,
            job.NetPerDeposit.Amount,
            job.DepositAccountId,
            paymentDays,
            job.Deductions.Select(DeductionDto.FromEntity).ToList(),
            PaymentSchedule.NextPaymentDates(
                DateOnly.FromDateTime(DateTime.UtcNow), paymentDays, 3),
            job.CreatedAt);
    }
}