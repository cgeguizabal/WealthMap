using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.MarkFreelanceJobPaid;

/// <summary>
/// The one place freelance work touches money.
/// </summary>
/// <remarks>
/// Three things happen together: the job records the payment, the account gains
/// the balance, and a movement records why. Any one of them alone is a lie —
/// a paid job with no deposit, or a balance that rose for no stated reason — so
/// they run inside a transaction.
/// </remarks>
public class MarkFreelanceJobPaidHandler : ICommandHandler<MarkFreelanceJobPaidCommand, FreelanceJobDto>
{
    private readonly IFreelanceJobRepository _jobs;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IUnitOfWork _unitOfWork;

    public MarkFreelanceJobPaidHandler(
        IFreelanceJobRepository jobs,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _accounts = accounts;
        _movements = movements;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreelanceJobDto> Handle(MarkFreelanceJobPaidCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Freelance job", request.Id);

        var account = await _accounts.GetByIdForUserAsync(request.DepositAccountId, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.DepositAccountId);

        // Checked here rather than left to the entity so the message names both
        // currencies. A client paying in a currency the chosen account cannot
        // hold is a real mistake, not a validation technicality.
        if (account.Balance.Currency != job.AgreedAmount.Currency)
            throw new DomainException(
                $"This work is priced in {job.AgreedAmount.Currency} but the account holds " +
                $"{account.Balance.Currency}. Choose an account in {job.AgreedAmount.Currency}.");

        var amount = new Money(request.AmountPaid, job.AgreedAmount.Currency);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            job.MarkPaid(amount, account.Id, request.PaidOn);
            account.Deposit(amount);

            await _movements.AddAsync(new AccountMovement(
                account.Id,
                request.UserId,
                MovementType.FreelanceIncome,
                amount,
                account.Balance,
                job.Title,
                DateTime.UtcNow,
                relatedEntityId: job.Id), ct);
        }, ct);

        return FreelanceJobDto.FromEntity(job);
    }
}
