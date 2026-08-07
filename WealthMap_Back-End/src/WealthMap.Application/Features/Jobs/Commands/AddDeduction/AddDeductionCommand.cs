using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Commands.AddDeduction;

public record AddDeductionCommand(
    Guid JobId,
    Guid UserId,
    string Name,
    int Type,
    decimal Value) : ICommand<JobDto>;