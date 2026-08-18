using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Queries.GetDebtById;

public class GetDebtByIdHandler : IQueryHandler<GetDebtByIdQuery, DebtDto>
{
    private readonly IDebtRepository _debts;
    private readonly IUserClock _clock;

    public GetDebtByIdHandler(IDebtRepository debts, IUserClock clock)
    {
        _debts = debts;
        _clock = clock;
    }

    public async Task<DebtDto> Handle(GetDebtByIdQuery request, CancellationToken ct)
    {
        var debt = await _debts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Debt", request.Id);

        return DebtDto.FromEntity(debt, _clock.Today);
    }
}
