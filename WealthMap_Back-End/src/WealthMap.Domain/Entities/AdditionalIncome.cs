using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// A recurring extra income (freelance, rent, …). One-time extras are just
/// Bonus-type account movements, not rows here.
/// </summary>
public class AdditionalIncome : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Money Amount { get; private set; }
    public IncomeFrequency Frequency { get; private set; }
    public Guid DepositAccountId { get; private set; }

    private AdditionalIncome()
    {
        Name = null!;
    }

    public AdditionalIncome(Guid userId, string name, Money amount, IncomeFrequency frequency, Guid depositAccountId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Additional income must belong to a user.");

        UserId = userId;
        Name = ValidateName(name);
        Amount = ValidateAmount(amount);
        Frequency = ValidateFrequency(frequency);
        DepositAccountId = ValidateAccount(depositAccountId);
    }

    public void Update(string name, Money amount, IncomeFrequency frequency, Guid depositAccountId)
    {
        if (amount.Currency != Amount.Currency)
            throw new DomainException("Cannot change the currency of an existing income.");

        Name = ValidateName(name);
        Amount = ValidateAmount(amount);
        Frequency = ValidateFrequency(frequency);
        DepositAccountId = ValidateAccount(depositAccountId);
        Touch();
    }

    private static string ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Income name is required.");

    private static Money ValidateAmount(Money amount) =>
        amount.IsZero || amount.IsNegative
            ? throw new DomainException("Income amount must be greater than zero.")
            : amount;

    private static IncomeFrequency ValidateFrequency(IncomeFrequency frequency) =>
        Enum.IsDefined(frequency)
            ? frequency
            : throw new DomainException("Frequency must be Weekly, Biweekly, Monthly or Yearly.");

    private static Guid ValidateAccount(Guid depositAccountId) =>
        depositAccountId == Guid.Empty
            ? throw new DomainException("A deposit account is required.")
            : depositAccountId;
}