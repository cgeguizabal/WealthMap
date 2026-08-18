using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetJobs;

public class GetJobsHandler : IQueryHandler<GetJobsQuery, IReadOnlyList<JobDto>>
{
    private readonly IJobRepository _jobs;
    private readonly IUserClock _clock;

    public GetJobsHandler(IJobRepository jobs, IUserClock clock)
    {
        _jobs = jobs;
        _clock = clock;
    }

    public async Task<IReadOnlyList<JobDto>> Handle(GetJobsQuery request, CancellationToken ct)
    {
        var jobs = await _jobs.GetAllForUserAsync(request.UserId, ct);
        return jobs.Select(d => JobDto.FromEntity(d, _clock.Today)).ToList();
    }
}