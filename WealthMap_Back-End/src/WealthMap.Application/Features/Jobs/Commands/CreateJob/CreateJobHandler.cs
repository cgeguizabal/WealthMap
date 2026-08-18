using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Jobs.Commands.CreateJob;

public class CreateJobHandler : ICommandHandler<CreateJobCommand, JobDto>
{
    private readonly IJobRepository _jobs;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public CreateJobHandler(IJobRepository jobs, IAccountRepository accounts, IUnitOfWork unitOfWork,
        IUserClock clock)
    {
        _jobs = jobs;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<JobDto> Handle(CreateJobCommand request, CancellationToken ct)
    {
        if (await _jobs.AnyForUserAsync(request.UserId, ct))
            throw new DomainException("You already have a job registered. Update or delete it first.");

        if (!await _accounts.ExistsForUserAsync(request.DepositAccountId, request.UserId, ct))
            throw new NotFoundException("Account", request.DepositAccountId);

        var job = new Job(
            request.UserId,
            request.Title,
            request.Employer,
            new Money(request.GrossMonthlySalary, request.Currency),
            request.DepositAccountId,
            request.PaymentDays);

        await _jobs.AddAsync(job, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return JobDto.FromEntity(job, _clock.Today);
    }
}