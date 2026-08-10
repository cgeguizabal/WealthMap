using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// An immutable record of one payment against a card, debt or installment plan,
/// whatever its source. Account-sourced payments also produce an AccountMovement;
/// External ones do not, which is precisely why this ledger exists — without it a
/// cash payment reduced a balance and left no trace behind.
/// </summary>
public class Payment : BaseEntity
{
    public Guid UserId { get; private set; }
    public PaymentTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentSourceType SourceType { get; private set; }
    public Guid? SourceAccountId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Notes { get; private set; }

    private Payment() { }

    public Payment(
        Guid userId,
        PaymentTargetType targetType,
        Guid targetId,
        Money amount,
        PaymentSourceType sourceType,
        Guid? sourceAccountId,
        DateTime occurredAt,
        string? notes = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Payment must belong to a user.");

        if (targetId == Guid.Empty)
            throw new DomainException("Payment must reference what it paid.");

        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Payment amount must be greater than zero.");

        if (occurredAt.Kind != DateTimeKind.Utc)
            throw new DomainException("Payment date must be UTC.");

        if (!Enum.IsDefined(targetType))
            throw new DomainException("Payment target must be CreditCard, Debt or Installment.");

        // The source and its account must agree: one implies the other's presence or absence.
        switch (sourceType)
        {
            case PaymentSourceType.Account when sourceAccountId is null || sourceAccountId == Guid.Empty:
                throw new DomainException("An account-sourced payment requires a source account.");
            case PaymentSourceType.External when sourceAccountId is not null:
                throw new DomainException("An external payment cannot reference an account.");
            default:
                if (!Enum.IsDefined(sourceType))
                    throw new DomainException("Payment source must be Account or External.");
                break;
        }

        UserId = userId;
        TargetType = targetType;
        TargetId = targetId;
        Amount = amount;
        SourceType = sourceType;
        SourceAccountId = sourceAccountId;
        OccurredAt = occurredAt;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}