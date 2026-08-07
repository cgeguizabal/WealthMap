using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;

namespace WealthMap.Application.Features.AdditionalIncomes.Queries.GetAdditionalIncomes;

public record GetAdditionalIncomesQuery(Guid UserId) : IQuery<IReadOnlyList<AdditionalIncomeDto>>;