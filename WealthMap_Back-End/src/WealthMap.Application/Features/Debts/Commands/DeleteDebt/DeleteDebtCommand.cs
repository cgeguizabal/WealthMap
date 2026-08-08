using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Debts.Commands.DeleteDebt;

public record DeleteDebtCommand(Guid Id, Guid UserId) : ICommand<bool>;
