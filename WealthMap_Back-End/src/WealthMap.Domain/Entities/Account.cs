using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string BankName { get; private set; }
    public AccountType Type { get; private set; }
    public Money Balance { get; private set; }
    public bool IsBlockedForSaving { get; private set; }
    public string? Notes { get; private set; }

    private Account()
{
    Name = null!;
    BankName = null!;
}

    public Account(Guid userId, string name, string bankName, AccountType type, Money openingBalance)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Account must belong to a user.");

        if (openingBalance.IsNegative)
            throw new DomainException("Opening balance cannot be negative.");

        UserId = userId;
        Name = ValidateName(name);
        BankName = ValidateBankName(bankName);
        Type = type;
        Balance = openingBalance;
        IsBlockedForSaving = false;
    }

    public void Deposit(Money amount)
    {
        EnsurePositive(amount);
        Balance = Balance + amount;
        Touch();
    }

    public void Withdraw(Money amount)
    {
        EnsurePositive(amount);

        if (IsBlockedForSaving)
            throw new DomainException(
                $"Account '{Name}' is blocked for saving. Unblock it before taking money out.");

        if (amount > Balance)
            throw new DomainException(
                $"Insufficient funds in '{Name}'. Available: {Balance}, requested: {amount}.");

        Balance = Balance - amount;
        Touch();
    }

    public void BlockForSaving()
    {
        if (IsBlockedForSaving)
            throw new DomainException($"Account '{Name}' is already blocked.");

        IsBlockedForSaving = true;
        Touch();
    }

    public void UnblockForSaving()
    {
        if (!IsBlockedForSaving)
            throw new DomainException($"Account '{Name}' is not blocked.");

        IsBlockedForSaving = false;
        Touch();
    }

    public void UpdateDetails(string name, string bankName, string? notes)
    {
        Name = ValidateName(name);
        BankName = ValidateBankName(bankName);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Touch();
    }

    private void EnsurePositive(Money amount)
    {
        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Amount must be greater than zero.");

        if (amount.Currency != Balance.Currency)
            throw new DomainException(
                $"Cannot operate in {amount.Currency} on an account held in {Balance.Currency}.");
    }

    private static string ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Account name is required.");

    private static string ValidateBankName(string bankName) =>
        !string.IsNullOrWhiteSpace(bankName)
            ? bankName.Trim()
            : throw new DomainException("Bank name is required.");
}