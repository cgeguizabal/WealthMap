using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Queries.GetFreelanceJobs;

public class GetFreelanceJobsHandler : IQueryHandler<GetFreelanceJobsQuery, IReadOnlyList<FreelanceJobDto>>
{
    private readonly IFreelanceJobRepository _jobs;

    public GetFreelanceJobsHandler(IFreelanceJobRepository jobs) => _jobs = jobs;

    public async Task<IReadOnlyList<FreelanceJobDto>> Handle(
        GetFreelanceJobsQuery request, CancellationToken ct)
    {
        var jobs = await _jobs.GetAllForUserAsync(request.UserId, ct);

        return jobs.Select(FreelanceJobDto.FromEntity).ToList();
    }
}
