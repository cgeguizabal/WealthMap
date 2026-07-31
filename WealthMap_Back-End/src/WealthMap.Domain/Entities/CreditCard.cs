using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

public class CreditCard : BaseEntity
{
    public Guid UserId { get; private set; }
    public string CardName { get; private set; }
    public string BankName { get; private set; }
    public Money CreditLimit { get; private set; }
    public Money UsedCredit { get; private set; }
    public decimal AnnualInterestRate { get; private set; }
    public int PaymentDueDay { get; private set; }
    public int StatementCutoffDay { get; private set; }
    public string? Notes { get; private set; }

    public Money AvailableCredit => CreditLimit - UsedCredit;

    private CreditCard() { }   // EF Core

    public CreditCard(
        Guid userId,
        string cardName,
        string bankName,
        Money creditLimit,
        decimal annualInterestRate,
        int paymentDueDay,
        int statementCutoffDay)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Credit card must belong to a user.");

        if (creditLimit.IsNegative || creditLimit.IsZero)
            throw new DomainException("Credit limit must be greater than zero.");

        UserId = userId;
        CardName = ValidateName(cardName, "Card name");
        BankName = ValidateName(bankName, "Bank name");
        CreditLimit = creditLimit;
        UsedCredit = Money.Zero(creditLimit.Currency);
        AnnualInterestRate = ValidateRate(annualInterestRate);
        PaymentDueDay = ValidateDayOfMonth(paymentDueDay, nameof(PaymentDueDay));
        StatementCutoffDay = ValidateDayOfMonth(statementCutoffDay, nameof(StatementCutoffDay));
    }

    public void Charge(Money amount)
    {
        EnsureValidAmount(amount);

        var newUsed = UsedCredit + amount;

        if (newUsed > CreditLimit)
            throw new DomainException(
                $"Charge declined on '{CardName}'. Available credit: {AvailableCredit}, requested: {amount}.");

        UsedCredit = newUsed;
        Touch();
    }

    public void RegisterPayment(Money amount)
    {
        EnsureValidAmount(amount);

        if (amount > UsedCredit)
            throw new DomainException(
                $"Payment exceeds the balance owed on '{CardName}'. Owed: {UsedCredit}, payment: {amount}.");

        UsedCredit = UsedCredit - amount;
        Touch();
    }

    public void UpdateCreditLimit(Money newLimit)
    {
        if (newLimit.Currency != CreditLimit.Currency)
            throw new DomainException("Cannot change the currency of an existing card.");

        if (newLimit < UsedCredit)
            throw new DomainException(
                $"New limit {newLimit} is below the current balance owed ({UsedCredit}).");

        CreditLimit = newLimit;
        Touch();
    }

    public void UpdateDetails(
        string cardName,
        string bankName,
        decimal annualInterestRate,
        int paymentDueDay,
        int statementCutoffDay,
        string? notes)
    {
        CardName = ValidateName(cardName, "Card name");
        BankName = ValidateName(bankName, "Bank name");
        AnnualInterestRate = ValidateRate(annualInterestRate);
        PaymentDueDay = ValidateDayOfMonth(paymentDueDay, nameof(PaymentDueDay));
        StatementCutoffDay = ValidateDayOfMonth(statementCutoffDay, nameof(StatementCutoffDay));
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Touch();
    }

    private void EnsureValidAmount(Money amount)
    {
        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Amount must be greater than zero.");

        if (amount.Currency != CreditLimit.Currency)
            throw new DomainException(
                $"Cannot operate in {amount.Currency} on a card held in {CreditLimit.Currency}.");
    }

    private static string ValidateName(string value, string field) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new DomainException($"{field} is required.");

    private static decimal ValidateRate(decimal rate) =>
        rate is >= 0 and <= 200
            ? rate
            : throw new DomainException("Annual interest rate must be between 0 and 200.");

    private static int ValidateDayOfMonth(int day, string field) =>
        day is >= 1 and <= 31
            ? day
            : throw new DomainException($"{field} must be between 1 and 31.");
}