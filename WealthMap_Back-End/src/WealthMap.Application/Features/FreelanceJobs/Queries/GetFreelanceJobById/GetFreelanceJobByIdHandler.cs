using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Queries.GetFreelanceJobById;

public class GetFreelanceJobByIdHandler : IQueryHandler<GetFreelanceJobByIdQuery, FreelanceJobDto>
{
    private readonly IFreelanceJobRepository _jobs;

    public GetFreelanceJobByIdHandler(IFreelanceJobRepository jobs) => _jobs = jobs;

    public async Task<FreelanceJobDto> Handle(GetFreelanceJobByIdQuery request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Freelance job", request.Id);

        return FreelanceJobDto.FromEntity(job);
    }
}
