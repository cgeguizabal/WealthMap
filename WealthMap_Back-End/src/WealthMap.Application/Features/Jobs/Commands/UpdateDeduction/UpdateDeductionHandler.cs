using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Jobs.Commands.UpdateDeduction;

public class UpdateDeductionHandler : ICommandHandler<UpdateDeductionCommand, JobDto>
{
    private readonly IJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public UpdateDeductionHandler(IJobRepository jobs, IUnitOfWork unitOfWork,
        IUserClock clock)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<JobDto> Handle(UpdateDeductionCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.JobId, request.UserId, ct)
            ?? throw new NotFoundException("Job", request.JobId);

        if (!job.HasDeduction(request.DeductionId))
            throw new NotFoundException("Deduction", request.DeductionId);

        job.UpdateDeduction(request.DeductionId, request.Name, (DeductionType)request.Type, request.Value);

        await _unitOfWork.SaveChangesAsync(ct);

        return JobDto.FromEntity(job, _clock.Today);
    }
}