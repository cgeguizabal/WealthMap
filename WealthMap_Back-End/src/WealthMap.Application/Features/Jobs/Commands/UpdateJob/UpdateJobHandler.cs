using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Jobs.Commands.UpdateJob;

public class UpdateJobHandler : ICommandHandler<UpdateJobCommand, JobDto>
{
    private readonly IJobRepository _jobs;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public UpdateJobHandler(IJobRepository jobs, IAccountRepository accounts, IUnitOfWork unitOfWork,
        IUserClock clock)
    {
        _jobs = jobs;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<JobDto> Handle(UpdateJobCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Job", request.Id);

        if (!await _accounts.ExistsForUserAsync(request.DepositAccountId, request.UserId, ct))
            throw new NotFoundException("Account", request.DepositAccountId);

        job.UpdateDetails(
            request.Title,
            request.Employer,
            new Money(request.GrossMonthlySalary, job.GrossMonthlySalary.Currency),
            request.DepositAccountId);

        job.SetPaymentDays(request.PaymentDays);

        await _unitOfWork.SaveChangesAsync(ct);

        return JobDto.FromEntity(job, _clock.Today);
    }
}