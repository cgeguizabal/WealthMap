using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.UpdateFreelanceJob;

public class UpdateFreelanceJobHandler : ICommandHandler<UpdateFreelanceJobCommand, FreelanceJobDto>
{
    private readonly IFreelanceJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFreelanceJobHandler(IFreelanceJobRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreelanceJobDto> Handle(UpdateFreelanceJobCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Freelance job", request.Id);

        job.Update(
            request.Title,
            new Money(request.AgreedAmount, request.Currency),
            request.Client,
            request.DueOn,
            request.Notes);

        await _unitOfWork.SaveChangesAsync(ct);

        return FreelanceJobDto.FromEntity(job);
    }
}
