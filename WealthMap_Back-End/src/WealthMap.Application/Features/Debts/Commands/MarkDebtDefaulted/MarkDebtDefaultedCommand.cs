using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Commands.MarkDebtDefaulted;

public record MarkDebtDefaultedCommand(Guid Id, Guid UserId) : ICommand<DebtDto>;
