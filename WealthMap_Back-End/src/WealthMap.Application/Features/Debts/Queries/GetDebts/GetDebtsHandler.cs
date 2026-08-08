using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Queries.GetDebts;

public class GetDebtsHandler : IQueryHandler<GetDebtsQuery, IReadOnlyList<DebtDto>>
{
    private readonly IDebtRepository _debts;

    public GetDebtsHandler(IDebtRepository debts) => _debts = debts;

    public async Task<IReadOnlyList<DebtDto>> Handle(GetDebtsQuery request, CancellationToken ct)
    {
        var debts = await _debts.GetAllForUserAsync(request.UserId, ct);
        return debts.Select(DebtDto.FromEntity).ToList();
    }
}
