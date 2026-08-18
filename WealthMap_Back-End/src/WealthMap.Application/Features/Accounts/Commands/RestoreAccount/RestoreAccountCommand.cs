using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Accounts.Commands.RestoreAccount;

/// <summary>Brings an archived account back into the lists and the totals.</summary>
public record RestoreAccountCommand(Guid Id, Guid UserId) : ICommand<bool>;
