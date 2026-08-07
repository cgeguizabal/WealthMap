using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Commands.RemoveDeduction;

public record RemoveDeductionCommand(
    Guid JobId,
    Guid DeductionId,
    Guid UserId) : ICommand<JobDto>;