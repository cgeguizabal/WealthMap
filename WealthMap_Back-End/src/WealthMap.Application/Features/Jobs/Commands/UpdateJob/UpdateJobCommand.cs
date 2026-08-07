using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Commands.UpdateJob;

public record UpdateJobCommand(
    Guid Id,
    Guid UserId,
    string Title,
    string Employer,
    decimal GrossMonthlySalary,
    Guid DepositAccountId,
    IReadOnlyList<int> PaymentDays) : ICommand<JobDto>;