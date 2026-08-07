using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Commands.UpdateDeduction;

public record UpdateDeductionCommand(
    Guid JobId,
    Guid DeductionId,
    Guid UserId,
    string Name,
    int Type,
    decimal Value) : ICommand<JobDto>;