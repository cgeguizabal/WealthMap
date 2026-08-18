using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetJobById;

public class GetJobByIdHandler : IQueryHandler<GetJobByIdQuery, JobDto>
{
    private readonly IJobRepository _jobs;
    private readonly IUserClock _clock;

    public GetJobByIdHandler(IJobRepository jobs, IUserClock clock)
    {
        _jobs = jobs;
        _clock = clock;
    }

    public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Job", request.Id);

        return JobDto.FromEntity(job, _clock.Today);
    }
}