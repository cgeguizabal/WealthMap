using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// A loan or personal debt. Original amount is the immutable fact; remaining
/// shrinks with payments. Status transitions: Active → PaidOff at zero,
/// Active → Defaulted manually, Defaulted → Active by paying again.
/// </summary>
public class Debt : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Money OriginalAmount { get; private set; }
    public Money RemainingAmount { get; private set; }
    public Money MonthlyPayment { get; private set; }
    public int MonthlyDueDay { get; private set; }
    public DebtStatus Status { get; private set; }

    private Debt()
    {
        Name = null!;
    }

    public Debt(
        Guid userId,
        string name,
        Money originalAmount,
        Money remainingAmount,
        Money monthlyPayment,
        int monthlyDueDay)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Debt must belong to a user.");

        if (originalAmount.IsZero || originalAmount.IsNegative)
            throw new DomainException("Original amount must be greater than zero.");

        if (remainingAmount.IsZero || remainingAmount.IsNegative)
            throw new DomainException("Remaining amount must be greater than zero.");

        if (remainingAmount > originalAmount)
            throw new DomainException("Remaining amount cannot exceed the original amount.");

        UserId = userId;
        Name = ValidateName(name);
        OriginalAmount = originalAmount;
        RemainingAmount = remainingAmount;
        MonthlyPayment = ValidateMonthlyPayment(monthlyPayment, originalAmount.Currency);
        MonthlyDueDay = ValidateDueDay(monthlyDueDay);
        Status = DebtStatus.Active;
    }

    public void RegisterPayment(Money amount)
    {
        if (Status == DebtStatus.PaidOff)
            throw new DomainException($"'{Name}' is already paid off.");

        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Payment amount must be greater than zero.");

        if (amount > RemainingAmount)
            throw new DomainException(
                $"Payment exceeds the remaining debt on '{Name}'. Remaining: {RemainingAmount}, payment: {amount}.");

        RemainingAmount = RemainingAmount - amount;

        Status = RemainingAmount.IsZero
            ? DebtStatus.PaidOff
            : DebtStatus.Active;   // paying a defaulted debt reactivates it

        Touch();
    }

    public void MarkDefaulted()
    {
        if (Status != DebtStatus.Active)
            throw new DomainException($"Only an active debt can be marked as defaulted. '{Name}' is {Status}.");

        Status = DebtStatus.Defaulted;
        Touch();
    }

    public void UpdateDetails(string name, Money monthlyPayment, int monthlyDueDay)
    {
        Name = ValidateName(name);
        MonthlyPayment = ValidateMonthlyPayment(monthlyPayment, OriginalAmount.Currency);
        MonthlyDueDay = ValidateDueDay(monthlyDueDay);
        Touch();
    }

    private static string ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Debt name is required.");

    private static Money ValidateMonthlyPayment(Money monthlyPayment, string currency)
    {
        if (monthlyPayment.IsZero || monthlyPayment.IsNegative)
            throw new DomainException("Monthly payment must be greater than zero.");

        if (monthlyPayment.Currency != currency)
            throw new DomainException("Monthly payment must be in the debt's currency.");

        return monthlyPayment;
    }

    private static int ValidateDueDay(int day) =>
        day is >= 1 and <= 31
            ? day
            : throw new DomainException("Monthly due day must be between 1 and 31.");
}
