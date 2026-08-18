using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.CreateFreelanceJob;

public class CreateFreelanceJobHandler : ICommandHandler<CreateFreelanceJobCommand, FreelanceJobDto>
{
    private readonly IFreelanceJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFreelanceJobHandler(IFreelanceJobRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreelanceJobDto> Handle(CreateFreelanceJobCommand request, CancellationToken ct)
    {
        // No deposit account here on purpose: where the money lands is decided
        // when it actually arrives, which may be months later and may not be the
        // account the user had in mind when the work was agreed.
        var job = new FreelanceJob(
            request.UserId,
            request.Title,
            new Money(request.AgreedAmount, request.Currency),
            request.Client,
            request.DueOn,
            request.Notes);

        await _jobs.AddAsync(job, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return FreelanceJobDto.FromEntity(job);
    }
}
