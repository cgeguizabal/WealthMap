using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetSalaryDeposits;

public class GetSalaryDepositsHandler
    : IQueryHandler<GetSalaryDepositsQuery, IReadOnlyList<SalaryDepositDto>>
{
    private readonly IJobRepository _jobs;
    private readonly ISalaryDepositRepository _deposits;

    public GetSalaryDepositsHandler(IJobRepository jobs, ISalaryDepositRepository deposits)
    {
        _jobs = jobs;
        _deposits = deposits;
    }

    public async Task<IReadOnlyList<SalaryDepositDto>> Handle(
        GetSalaryDepositsQuery request, CancellationToken ct)
    {
        if (await _jobs.GetByIdForUserAsync(request.JobId, request.UserId, ct) is null)
            throw new NotFoundException("Job", request.JobId);

        var deposits = await _deposits.GetForJobAsync(request.JobId, request.UserId, ct);
        return deposits.Select(SalaryDepositDto.FromEntity).ToList();
    }
}
