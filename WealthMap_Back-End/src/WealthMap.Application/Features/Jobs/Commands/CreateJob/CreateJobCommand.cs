using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Commands.CreateJob;

public record CreateJobCommand(
    Guid UserId,
    string Title,
    string Employer,
    decimal GrossMonthlySalary,
    string Currency,
    Guid DepositAccountId,
    IReadOnlyList<int> PaymentDays) : ICommand<JobDto>;