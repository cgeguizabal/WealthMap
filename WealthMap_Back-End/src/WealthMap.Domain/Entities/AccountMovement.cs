using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

public class AccountMovement : BaseEntity
{
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public MovementType Type { get; private set; }
    public Money Amount { get; private set; }
    public Money BalanceAfter { get; private set; }
    public string Description { get; private set; }
    public string? Location { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    public bool IsInbound => Type is MovementType.SalaryDeposit
                                  or MovementType.Deposit
                                  or MovementType.Bonus
                                  or MovementType.TransferIn;

    private AccountMovement() { }   // EF Core

    public AccountMovement(
        Guid accountId,
        Guid userId,
        MovementType type,
        Money amount,
        Money balanceAfter,
        string description,
        DateTime occurredAt,
        Guid? relatedEntityId = null,
        string? location = null)
    {
        if (accountId == Guid.Empty)
            throw new DomainException("Movement must belong to an account.");

        if (userId == Guid.Empty)
            throw new DomainException("Movement must belong to a user.");

        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Movement amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Movement description is required.");

        if (occurredAt.Kind != DateTimeKind.Utc)
            throw new DomainException("Movement date must be UTC.");

        AccountId = accountId;
        UserId = userId;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        Description = description.Trim();
        OccurredAt = occurredAt;
        RelatedEntityId = relatedEntityId;
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
    }
}