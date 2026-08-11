using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// A record that one payday has been paid into the account. This is what makes
/// automatic posting safe to run repeatedly: the poster reads which dates already
/// have a row and skips them, so a restart, a second instance or a manual run
/// cannot pay the same day twice.
/// </summary>
/// <remarks>
/// A unique index on (job_id, scheduled_date) enforces this in the database as
/// well, because two instances can pass the "already posted?" check at once.
/// </remarks>
public class SalaryDeposit : BaseEntity
{
    public Guid JobId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }

    /// <summary>The payday this row settles — not the moment it was written.</summary>
    public DateOnly ScheduledDate { get; private set; }

    public Money Amount { get; private set; }
    public DateTime PostedAt { get; private set; }

    /// <summary>The movement written into the account, so the two can be reconciled.</summary>
    public Guid AccountMovementId { get; private set; }

    private SalaryDeposit() { }

    public SalaryDeposit(
        Guid jobId,
        Guid userId,
        Guid accountId,
        DateOnly scheduledDate,
        Money amount,
        Guid accountMovementId)
    {
        if (jobId == Guid.Empty)
            throw new DomainException("Salary deposit must belong to a job.");

        if (userId == Guid.Empty)
            throw new DomainException("Salary deposit must belong to a user.");

        if (accountId == Guid.Empty)
            throw new DomainException("Salary deposit must name the account it was paid into.");

        if (accountMovementId == Guid.Empty)
            throw new DomainException("Salary deposit must reference its account movement.");

        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Salary deposit amount must be greater than zero.");

        JobId = jobId;
        UserId = userId;
        AccountId = accountId;
        ScheduledDate = scheduledDate;
        Amount = amount;
        AccountMovementId = accountMovementId;
        PostedAt = DateTime.UtcNow;
    }
}
