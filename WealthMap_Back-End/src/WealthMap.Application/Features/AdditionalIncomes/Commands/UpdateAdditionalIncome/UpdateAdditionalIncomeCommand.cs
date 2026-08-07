using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;

namespace WealthMap.Application.Features.AdditionalIncomes.Commands.UpdateAdditionalIncome;

public record UpdateAdditionalIncomeCommand(
    Guid Id,
    Guid UserId,
    string Name,
    decimal Amount,
    int Frequency,
    Guid DepositAccountId) : ICommand<AdditionalIncomeDto>;