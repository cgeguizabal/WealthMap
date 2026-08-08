using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Commands.CreateDebt;

public record CreateDebtCommand(
    Guid UserId,
    string Name,
    decimal OriginalAmount,
    decimal? RemainingAmount,
    string Currency,
    decimal MonthlyPayment,
    int MonthlyDueDay) : ICommand<DebtDto>;
