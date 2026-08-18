using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// Immutable record of a purchase. The payment method decides which instrument
/// reference is required: DebitAccount → AccountId, CreditCard → CreditCardId,
/// Cash → neither (cash exits tracking by design).
/// </summary>
public class Purchase : BaseEntity
{
    public Guid UserId { get; private set; }
    public string ProductName { get; private set; }
    public Money Amount { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid? StoreId { get; private set; }
    public string Category { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public Guid? AccountId { get; private set; }
    public Guid? CreditCardId { get; private set; }
    public string? Notes { get; private set; }

    private Purchase()
    {
        ProductName = null!;
        Category = null!;
    }

    public Purchase(
        Guid userId,
        string productName,
        Money amount,
        DateTime occurredAt,
        Guid? storeId,
        string category,
        PaymentMethod paymentMethod,
        Guid? accountId,
        Guid? creditCardId,
        string? notes)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Purchase must belong to a user.");

        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Purchase amount must be greater than zero.");

        if (occurredAt.Kind != DateTimeKind.Utc)
            throw new DomainException("Purchase date must be UTC.");

        EnsureInstrumentMatchesMethod(paymentMethod, accountId, creditCardId);

        UserId = userId;
        ProductName = ValidateText(productName, "Product name");
        Amount = amount;
        OccurredAt = occurredAt;
        StoreId = storeId;
        Category = ValidateText(category, "Category");
        PaymentMethod = paymentMethod;
        AccountId = accountId;
        CreditCardId = creditCardId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    /// <summary>
    /// Corrects a purchase that was recorded wrongly.
    /// </summary>
    /// <remarks>
    /// Every field is editable, including the payment method and the instrument,
    /// because "I put it on the wrong card" is the correction people actually need
    /// and forcing a delete-and-retype to get it would lose the record's identity.
    ///
    /// This changes the *record* only. The money it moved is reversed and reapplied
    /// by the caller, which is the one place that can see the account and card
    /// involved — an entity cannot reach across to another aggregate to do it.
    /// </remarks>
    public void Update(
        string productName,
        Money amount,
        DateTime occurredAt,
        Guid? storeId,
        string category,
        PaymentMethod paymentMethod,
        Guid? accountId,
        Guid? creditCardId,
        string? notes)
    {
        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Purchase amount must be greater than zero.");

        if (occurredAt.Kind != DateTimeKind.Utc)
            throw new DomainException("Purchase date must be UTC.");

        EnsureInstrumentMatchesMethod(paymentMethod, accountId, creditCardId);

        ProductName = ValidateText(productName, "Product name");
        Amount = amount;
        OccurredAt = occurredAt;
        StoreId = storeId;
        Category = ValidateText(category, "Category");
        PaymentMethod = paymentMethod;
        AccountId = accountId;
        CreditCardId = creditCardId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        Touch();
    }

    /// <summary>
    /// A purchase names exactly the instrument its method implies, and no other.
    /// </summary>
    /// <remarks>
    /// Shared by the constructor and <see cref="Update"/>. Two copies would let a
    /// correction write a combination creation refuses — a debit purchase pointing
    /// at a credit card, say — and nothing downstream is prepared for that.
    /// </remarks>
    private static void EnsureInstrumentMatchesMethod(
        PaymentMethod paymentMethod, Guid? accountId, Guid? creditCardId)
    {
        switch (paymentMethod)
        {
            case PaymentMethod.DebitAccount when accountId is null || accountId == Guid.Empty:
                throw new DomainException("A debit purchase requires an account.");
            case PaymentMethod.DebitAccount when creditCardId is not null:
                throw new DomainException("A debit purchase cannot reference a credit card.");
            case PaymentMethod.CreditCard when creditCardId is null || creditCardId == Guid.Empty:
                throw new DomainException("A credit purchase requires a credit card.");
            case PaymentMethod.CreditCard when accountId is not null:
                throw new DomainException("A credit purchase cannot reference an account.");
            case PaymentMethod.Cash when accountId is not null || creditCardId is not null:
                throw new DomainException("A cash purchase cannot reference an account or card.");
            default:
                if (!Enum.IsDefined(paymentMethod))
                    throw new DomainException("Payment method must be DebitAccount, CreditCard or Cash.");
                break;
        }
    }

    private static string ValidateText(string value, string field) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new DomainException($"{field} is required.");
}
