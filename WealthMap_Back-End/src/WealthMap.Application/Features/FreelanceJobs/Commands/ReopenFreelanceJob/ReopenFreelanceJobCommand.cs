using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.ReopenFreelanceJob;

/// <summary>
/// Un-cancels work the client came back for. Only cancelled work can be reopened.
/// </summary>
public record ReopenFreelanceJobCommand(Guid Id, Guid UserId) : ICommand<FreelanceJobDto>;
