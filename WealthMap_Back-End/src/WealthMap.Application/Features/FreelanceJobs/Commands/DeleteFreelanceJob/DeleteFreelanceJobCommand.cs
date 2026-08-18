using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.DeleteFreelanceJob;

public record DeleteFreelanceJobCommand(Guid Id, Guid UserId) : ICommand<bool>;
