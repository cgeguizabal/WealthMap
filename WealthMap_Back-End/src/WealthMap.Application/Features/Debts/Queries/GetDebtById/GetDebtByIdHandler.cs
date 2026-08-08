using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Queries.GetDebtById;

public class GetDebtByIdHandler : IQueryHandler<GetDebtByIdQuery, DebtDto>
{
    private readonly IDebtRepository _debts;

    public GetDebtByIdHandler(IDebtRepository debts) => _debts = debts;

    public async Task<DebtDto> Handle(GetDebtByIdQuery request, CancellationToken ct)
    {
        var debt = await _debts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Debt", request.Id);

        return DebtDto.FromEntity(debt);
    }
}
