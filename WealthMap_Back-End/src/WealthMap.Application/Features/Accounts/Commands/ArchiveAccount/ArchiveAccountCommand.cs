using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Accounts.Commands.ArchiveAccount;

public record ArchiveAccountCommand(Guid Id, Guid UserId) : ICommand<bool>;
