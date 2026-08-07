using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;

namespace WealthMap.Application.Features.AdditionalIncomes.Queries.GetAdditionalIncomes;

public class GetAdditionalIncomesHandler : IQueryHandler<GetAdditionalIncomesQuery, IReadOnlyList<AdditionalIncomeDto>>
{
    private readonly IAdditionalIncomeRepository _incomes;

    public GetAdditionalIncomesHandler(IAdditionalIncomeRepository incomes) => _incomes = incomes;

    public async Task<IReadOnlyList<AdditionalIncomeDto>> Handle(GetAdditionalIncomesQuery request, CancellationToken ct)
    {
        var incomes = await _incomes.GetAllForUserAsync(request.UserId, ct);
        return incomes.Select(AdditionalIncomeDto.FromEntity).ToList();
    }
}