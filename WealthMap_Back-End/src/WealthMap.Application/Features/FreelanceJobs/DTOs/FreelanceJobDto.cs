using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.FreelanceJobs.DTOs;

/// <summary>
/// One piece of freelance work as the client sees it.
/// </summary>
/// <remarks>
/// <c>Status</c> and <c>Outstanding</c> are sent as values even though the entity
/// computes them. The client would otherwise have to reimplement the same rules
/// in JavaScript, and the two would drift the first time one changed.
/// </remarks>
public record FreelanceJobDto(
    Guid Id,
    string Title,
    string? Client,
    decimal AgreedAmount,
    decimal AmountPaid,
    decimal Outstanding,
    string Currency,
    string Status,
    DateOnly? DueOn,
    DateOnly? DeliveredOn,
    DateOnly? PaidOn,
    DateOnly? CancelledOn,
    Guid? DepositAccountId,
    string? Notes)
{
    public static FreelanceJobDto FromEntity(FreelanceJob job) => new(
        job.Id,
        job.Title,
        job.Client,
        job.AgreedAmount.Amount,
        job.AmountPaid.Amount,
        job.Outstanding.Amount,
        job.AgreedAmount.Currency,
        job.Status.ToString(),
        job.DueOn,
        job.DeliveredOn,
        job.PaidOn,
        job.CancelledOn,
        job.DepositAccountId,
        job.Notes);
}
