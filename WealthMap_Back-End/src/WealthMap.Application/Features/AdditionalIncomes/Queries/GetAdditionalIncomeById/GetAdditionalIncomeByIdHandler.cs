using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;

namespace WealthMap.Application.Features.AdditionalIncomes.Queries.GetAdditionalIncomeById;

public class GetAdditionalIncomeByIdHandler : IQueryHandler<GetAdditionalIncomeByIdQuery, AdditionalIncomeDto>
{
    private readonly IAdditionalIncomeRepository _incomes;

    public GetAdditionalIncomeByIdHandler(IAdditionalIncomeRepository incomes) => _incomes = incomes;

    public async Task<AdditionalIncomeDto> Handle(GetAdditionalIncomeByIdQuery request, CancellationToken ct)
    {
        var income = await _incomes.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("AdditionalIncome", request.Id);

        return AdditionalIncomeDto.FromEntity(income);
    }
}