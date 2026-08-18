using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Queries.GetDebts;

public class GetDebtsHandler : IQueryHandler<GetDebtsQuery, IReadOnlyList<DebtDto>>
{
    private readonly IDebtRepository _debts;
    private readonly IUserClock _clock;

    public GetDebtsHandler(IDebtRepository debts, IUserClock clock)
    {
        _debts = debts;
        _clock = clock;
    }

    public async Task<IReadOnlyList<DebtDto>> Handle(GetDebtsQuery request, CancellationToken ct)
    {
        var debts = await _debts.GetAllForUserAsync(request.UserId, ct);
        return debts.Select(d => DebtDto.FromEntity(d, _clock.Today)).ToList();
    }
}
