using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.MarkFreelanceJobDelivered;

public class MarkFreelanceJobDeliveredHandler
    : ICommandHandler<MarkFreelanceJobDeliveredCommand, FreelanceJobDto>
{
    private readonly IFreelanceJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public MarkFreelanceJobDeliveredHandler(IFreelanceJobRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreelanceJobDto> Handle(
        MarkFreelanceJobDeliveredCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Freelance job", request.Id);

        // No money moves here. Delivering is a fact about the work, not about a
        // balance — the client may still take a month to pay.
        job.MarkDelivered(request.DeliveredOn);

        await _unitOfWork.SaveChangesAsync(ct);

        return FreelanceJobDto.FromEntity(job);
    }
}
