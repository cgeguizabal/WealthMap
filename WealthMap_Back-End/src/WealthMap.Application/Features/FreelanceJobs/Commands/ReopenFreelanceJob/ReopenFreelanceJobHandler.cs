using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.ReopenFreelanceJob;

public class ReopenFreelanceJobHandler : ICommandHandler<ReopenFreelanceJobCommand, FreelanceJobDto>
{
    private readonly IFreelanceJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public ReopenFreelanceJobHandler(IFreelanceJobRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreelanceJobDto> Handle(ReopenFreelanceJobCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Freelance job", request.Id);

        // Clears the cancellation date, which puts the work back wherever its
        // other dates say it belongs — delivered if it had been, in progress if
        // not. Nothing else is touched.
        job.Reopen();

        await _unitOfWork.SaveChangesAsync(ct);

        return FreelanceJobDto.FromEntity(job);
    }
}
