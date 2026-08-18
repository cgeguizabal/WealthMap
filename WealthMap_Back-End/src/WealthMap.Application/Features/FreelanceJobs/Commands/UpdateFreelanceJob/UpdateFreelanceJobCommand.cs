using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.UpdateFreelanceJob;

public record UpdateFreelanceJobCommand(
    Guid Id,
    Guid UserId,
    string Title,
    decimal AgreedAmount,
    string Currency,
    string? Client,
    DateOnly? DueOn,
    string? Notes) : ICommand<FreelanceJobDto>;
