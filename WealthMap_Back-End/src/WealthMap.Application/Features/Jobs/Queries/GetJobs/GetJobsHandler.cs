using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetJobs;

public class GetJobsHandler : IQueryHandler<GetJobsQuery, IReadOnlyList<JobDto>>
{
    private readonly IJobRepository _jobs;

    public GetJobsHandler(IJobRepository jobs) => _jobs = jobs;

    public async Task<IReadOnlyList<JobDto>> Handle(GetJobsQuery request, CancellationToken ct)
    {
        var jobs = await _jobs.GetAllForUserAsync(request.UserId, ct);
        return jobs.Select(JobDto.FromEntity).ToList();
    }
}