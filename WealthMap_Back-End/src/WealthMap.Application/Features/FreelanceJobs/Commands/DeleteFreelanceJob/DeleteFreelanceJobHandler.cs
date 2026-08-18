using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.FreelanceJobs.Commands.DeleteFreelanceJob;

/// <summary>
/// Removes the record, reversing the deposit if one was made.
/// </summary>
/// <remarks>
/// Mirrors purchase deletion: a hard delete that undoes everything the record
/// did, rather than a soft flag. The reasoning is the same — this data is typed
/// by hand, so a wrong entry is a normal event, and a delete that left the money
/// behind would force the user to correct the balance manually and would put the
/// two records permanently out of step.
///
/// Cancelling is the other option and means something different: the work was
/// called off. That keeps the row.
/// </remarks>
public class DeleteFreelanceJobHandler : ICommandHandler<DeleteFreelanceJobCommand, bool>
{
    private readonly IFreelanceJobRepository _jobs;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFreelanceJobHandler(
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

    public async Task<bool> Handle(DeleteFreelanceJobCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Freelance job", request.Id);

        // Never paid: nothing moved, so nothing to undo.
        if (job.Status != FreelanceJobStatus.Paid)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _jobs.Remove(job);
                await Task.CompletedTask;
            }, ct);

            return true;
        }

        var account = await _accounts.GetByIdForUserAsync(job.DepositAccountId!.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", job.DepositAccountId.Value);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Throws if the money has already been spent below this amount, which
            // is the honest failure: the deposit cannot be taken back out of an
            // account that no longer holds it.
            account.Withdraw(job.AmountPaid);

            var movement = await _movements.GetByRelatedEntityAsync(job.Id, request.UserId, ct);

            if (movement is not null)
            {
                // Every later movement recorded a running balance that assumed
                // this deposit happened. Removing it without rebasing them would
                // leave the history visibly failing to add up.
                var later = await _movements.GetForAccountAfterAsync(
                    account.Id, request.UserId, movement.OccurredAt, ct);

                foreach (var subsequent in later)
                    subsequent.RebaseBalanceAfter(-job.AmountPaid);

                _movements.Remove(movement);
            }

            _jobs.Remove(job);
        }, ct);

        return true;
    }
}
