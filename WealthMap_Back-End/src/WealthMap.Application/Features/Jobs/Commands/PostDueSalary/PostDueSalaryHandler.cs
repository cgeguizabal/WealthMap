using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;

namespace WealthMap.Application.Features.Jobs.Commands.PostDueSalary;

public class PostDueSalaryHandler : ICommandHandler<PostDueSalaryCommand, int>
{
    private readonly IJobRepository _jobs;
    private readonly SalaryPostingService _poster;

    public PostDueSalaryHandler(IJobRepository jobs, SalaryPostingService poster)
    {
        _jobs = jobs;
        _poster = poster;
    }

    public async Task<int> Handle(PostDueSalaryCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.JobId, request.UserId, ct)
            ?? throw new NotFoundException("Job", request.JobId);

        return await _poster.PostDueForJobAsync(job, DateOnly.FromDateTime(DateTime.UtcNow), ct);
    }
}
