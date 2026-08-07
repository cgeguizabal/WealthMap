using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Commands.RemoveDeduction;

public class RemoveDeductionHandler : ICommandHandler<RemoveDeductionCommand, JobDto>
{
    private readonly IJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveDeductionHandler(IJobRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<JobDto> Handle(RemoveDeductionCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.JobId, request.UserId, ct)
            ?? throw new NotFoundException("Job", request.JobId);

        if (!job.HasDeduction(request.DeductionId))
            throw new NotFoundException("Deduction", request.DeductionId);

        job.RemoveDeduction(request.DeductionId);

        await _unitOfWork.SaveChangesAsync(ct);

        return JobDto.FromEntity(job);
    }
}