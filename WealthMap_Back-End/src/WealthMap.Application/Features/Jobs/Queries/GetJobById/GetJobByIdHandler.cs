using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetJobById;

public class GetJobByIdHandler : IQueryHandler<GetJobByIdQuery, JobDto>
{
    private readonly IJobRepository _jobs;

    public GetJobByIdHandler(IJobRepository jobs) => _jobs = jobs;

    public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Job", request.Id);

        return JobDto.FromEntity(job);
    }
}