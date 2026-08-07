using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Jobs.Commands.DeleteJob;

public record DeleteJobCommand(Guid Id, Guid UserId) : ICommand<bool>;