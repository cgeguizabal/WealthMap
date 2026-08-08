using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// A savings target with a deadline. Optionally linked to a savings account —
/// contributions then move real money into that account; unlinked goals just
/// track amounts. Progress figures are computed, never stored.
/// </summary>
public class SavingsGoal : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Money TargetAmount { get; private set; }
    public Money CurrentAmount { get; private set; }
    public DateOnly Deadline { get; private set; }
    public Guid? LinkedAccountId { get; private set; }

    public decimal ProgressPercentage => GoalMath.ProgressPercentage(CurrentAmount, TargetAmount);

    public int? MonthsRemaining => GoalMath.MonthsRemaining(Today, Deadline);

    public Money? RequiredMonthlyContribution =>
        GoalMath.RequiredMonthlyContribution(CurrentAmount, TargetAmount, Today, Deadline);

    public GoalStatus Status => GoalMath.ComputeStatus(
        CurrentAmount, TargetAmount, Today, Deadline, DateOnly.FromDateTime(CreatedAt));

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private SavingsGoal()
    {
        Name = null!;
    }

    public SavingsGoal(
        Guid userId,
        string name,
        Money targetAmount,
        Money currentAmount,
        DateOnly deadline,
        Guid? linkedAccountId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Goal must belong to a user.");

        if (targetAmount.IsZero || targetAmount.IsNegative)
            throw new DomainException("Target amount must be greater than zero.");

        if (currentAmount.IsNegative)
            throw new DomainException("Current amount cannot be negative.");

        UserId = userId;
        Name = ValidateName(name);
        TargetAmount = targetAmount;
        CurrentAmount = currentAmount;
        Deadline = deadline;
        LinkedAccountId = linkedAccountId;
    }

    public void Contribute(Money amount)
    {
        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Contribution must be greater than zero.");

        CurrentAmount = CurrentAmount + amount;
        Touch();
    }

    public void UpdateDetails(string name, Money targetAmount, DateOnly deadline, Guid? linkedAccountId)
    {
        if (targetAmount.Currency != TargetAmount.Currency)
            throw new DomainException("Cannot change the currency of an existing goal.");

        if (targetAmount.IsZero || targetAmount.IsNegative)
            throw new DomainException("Target amount must be greater than zero.");

        Name = ValidateName(name);
        TargetAmount = targetAmount;
        Deadline = deadline;
        LinkedAccountId = linkedAccountId;
        Touch();
    }

    private static string ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Goal name is required.");
}
