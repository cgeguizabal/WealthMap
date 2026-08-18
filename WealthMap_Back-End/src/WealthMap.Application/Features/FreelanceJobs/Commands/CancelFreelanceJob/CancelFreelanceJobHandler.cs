using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.CancelFreelanceJob;

public class CancelFreelanceJobHandler : ICommandHandler<CancelFreelanceJobCommand, FreelanceJobDto>
{
    private readonly IFreelanceJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public CancelFreelanceJobHandler(IFreelanceJobRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreelanceJobDto> Handle(CancelFreelanceJobCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Freelance job", request.Id);

        job.Cancel(request.CancelledOn);

        await _unitOfWork.SaveChangesAsync(ct);

        return FreelanceJobDto.FromEntity(job);
    }
}
