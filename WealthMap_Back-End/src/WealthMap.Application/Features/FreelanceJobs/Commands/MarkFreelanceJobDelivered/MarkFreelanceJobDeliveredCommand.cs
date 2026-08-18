using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.MarkFreelanceJobDelivered;

public record MarkFreelanceJobDeliveredCommand(
    Guid Id,
    Guid UserId,
    DateOnly DeliveredOn) : ICommand<FreelanceJobDto>;
