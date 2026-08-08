using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Commands.PayDebt;

public record PayDebtCommand(
    Guid DebtId,
    Guid UserId,
    decimal Amount,
    string SourceType,
    Guid? SourceAccountId) : ICommand<DebtPaymentResultDto>;
