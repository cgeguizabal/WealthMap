using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;

namespace WealthMap.Application.Features.AdditionalIncomes.Commands.CreateAdditionalIncome;

public record CreateAdditionalIncomeCommand(
    Guid UserId,
    string Name,
    decimal Amount,
    string Currency,
    int Frequency,
    Guid DepositAccountId) : ICommand<AdditionalIncomeDto>;