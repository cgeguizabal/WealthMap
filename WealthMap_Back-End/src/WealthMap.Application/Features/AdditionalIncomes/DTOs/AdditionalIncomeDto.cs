using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.AdditionalIncomes.DTOs;

public record AdditionalIncomeDto(
    Guid Id,
    string Name,
    decimal Amount,
    string Currency,
    string Frequency,
    Guid DepositAccountId,
    DateTime CreatedAt)
{
    public static AdditionalIncomeDto FromEntity(AdditionalIncome income) => new(
        income.Id,
        income.Name,
        income.Amount.Amount,
        income.Amount.Currency,
        income.Frequency.ToString(),
        income.DepositAccountId,
        income.CreatedAt);
}