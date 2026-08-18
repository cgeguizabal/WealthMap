using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.CancelFreelanceJob;

public record CancelFreelanceJobCommand(
    Guid Id,
    Guid UserId,
    DateOnly CancelledOn) : ICommand<FreelanceJobDto>;
