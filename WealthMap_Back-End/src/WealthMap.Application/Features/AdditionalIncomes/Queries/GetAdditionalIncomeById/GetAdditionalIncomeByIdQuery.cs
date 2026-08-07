using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;

namespace WealthMap.Application.Features.AdditionalIncomes.Queries.GetAdditionalIncomeById;

public record GetAdditionalIncomeByIdQuery(Guid Id, Guid UserId) : IQuery<AdditionalIncomeDto>;