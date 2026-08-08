using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Commands.UpdateDebt;

public record UpdateDebtCommand(
    Guid Id,
    Guid UserId,
    string Name,
    decimal MonthlyPayment,
    int MonthlyDueDay) : ICommand<DebtDto>;
