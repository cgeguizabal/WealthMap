using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.BankDefaults.Commands.DeleteBankDefault;

public record DeleteBankDefaultCommand(Guid Id, Guid UserId) : ICommand<bool>;
