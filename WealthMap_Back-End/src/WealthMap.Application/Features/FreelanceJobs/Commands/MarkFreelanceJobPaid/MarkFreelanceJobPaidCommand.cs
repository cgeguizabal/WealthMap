using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.MarkFreelanceJobPaid;

/// <summary>
/// Records that a client paid. <paramref name="AmountPaid"/> is separate from the
/// agreed figure because clients round, deduct, or add a bonus.
/// </summary>
public record MarkFreelanceJobPaidCommand(
    Guid Id,
    Guid UserId,
    decimal AmountPaid,
    Guid DepositAccountId,
    DateOnly PaidOn) : ICommand<FreelanceJobDto>;
