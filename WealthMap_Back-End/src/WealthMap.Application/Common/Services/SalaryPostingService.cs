using Microsoft.Extensions.Logging;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Common.Services;

/// <summary>
/// Pays salary into the deposit account on the job's payment days.
/// </summary>
/// <remarks>
/// Written as catch-up rather than as a clock tick. Each run asks "which paydays
/// are due and unpaid?" and settles all of them, so the machine being asleep on
/// payday delays the deposit instead of losing it. The same property makes the
/// run safe to repeat: a settled payday is never selected again.
/// </remarks>
public class SalaryPostingService
{
    /// <summary>
    /// How far back a run will reach for unpaid paydays. Bounds the work after a
    /// long outage; <see cref="Job.SalaryPostingStartsOn"/> is the real floor.
    /// </summary>
    private const int CatchUpWindowDays = 400;

    private readonly IJobRepository _jobs;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly ISalaryDepositRepository _salaryDeposits;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SalaryPostingService> _logger;

    public SalaryPostingService(
        IJobRepository jobs,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        ISalaryDepositRepository salaryDeposits,
        IUnitOfWork unitOfWork,
        ILogger<SalaryPostingService> logger)
    {
        _jobs = jobs;
        _accounts = accounts;
        _movements = movements;
        _salaryDeposits = salaryDeposits;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Runs the catch-up for every job. Used by the scheduled runner.</summary>
    public async Task<int> PostAllDueAsync(DateOnly asOf, CancellationToken ct = default)
    {
        var jobs = await _jobs.GetAllForPostingAsync(ct);
        var posted = 0;

        foreach (var job in jobs)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                posted += await PostDueForJobAsync(job, asOf, ct);
            }
            catch (Exception ex)
            {
                // One user's job must not stop everyone else's salary.
                _logger.LogError(ex, "Salary posting failed for job {JobId}.", job.Id);
            }
        }

        return posted;
    }

    /// <summary>
    /// Settles every unpaid payday for one job up to <paramref name="asOf"/>.
    /// Returns how many deposits were written.
    /// </summary>
    public async Task<int> PostDueForJobAsync(Job job, DateOnly asOf, CancellationToken ct = default)
    {
        var from = asOf.AddDays(-CatchUpWindowDays);
        var due = job.ScheduledDatesBetween(from, asOf);

        if (due.Count == 0)
            return 0;

        var alreadyPosted = await _salaryDeposits.GetPostedDatesAsync(job.Id, from, asOf, ct);
        var outstanding = due.Except(alreadyPosted).ToList();

        if (outstanding.Count == 0)
            return 0;

        var account = await _accounts.GetByIdForUserAsync(job.DepositAccountId, job.UserId, ct);

        if (account is null)
        {
            _logger.LogWarning(
                "Job {JobId} deposit account {AccountId} no longer exists; skipping {Count} payday(s).",
                job.Id, job.DepositAccountId, outstanding.Count);
            return 0;
        }

        // Left unpaid rather than dropped: restoring the account lets the missed
        // paydays post on the next run.
        if (account.IsArchived)
        {
            _logger.LogWarning(
                "Job {JobId} deposit account {AccountId} is archived; holding {Count} payday(s).",
                job.Id, job.DepositAccountId, outstanding.Count);
            return 0;
        }

        var amount = job.NetPerDeposit;

        if (amount.Currency != account.Balance.Currency)
        {
            _logger.LogWarning(
                "Job {JobId} pays {JobCurrency} but account {AccountId} holds {AccountCurrency}; " +
                "holding {Count} payday(s) rather than converting.",
                job.Id, amount.Currency, account.Id, account.Balance.Currency, outstanding.Count);
            return 0;
        }

        if (amount.IsZero || amount.IsNegative)
        {
            _logger.LogWarning(
                "Job {JobId} nets {Amount} per deposit; nothing to post.", job.Id, amount);
            return 0;
        }

        var posted = 0;

        foreach (var payday in outstanding)
        {
            ct.ThrowIfCancellationRequested();

            // One transaction per payday: an outage midway through a catch-up
            // leaves earlier paydays settled instead of rolling all of them back.
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                account.Deposit(amount);

                var movement = new AccountMovement(
                    account.Id,
                    job.UserId,
                    MovementType.SalaryDeposit,
                    amount,
                    account.Balance,
                    $"Salary — {job.Employer}",
                    payday.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    relatedEntityId: job.Id);

                await _movements.AddAsync(movement, ct);

                await _salaryDeposits.AddAsync(
                    new SalaryDeposit(job.Id, job.UserId, account.Id, payday, amount, movement.Id), ct);
            }, ct);

            posted++;

            _logger.LogInformation(
                "Posted salary {Amount} to account {AccountId} for payday {Payday}.",
                amount, account.Id, payday);
        }

        return posted;
    }
}
