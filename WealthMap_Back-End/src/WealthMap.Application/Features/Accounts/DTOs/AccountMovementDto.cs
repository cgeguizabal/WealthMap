using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Accounts.DTOs;

public record AccountMovementDto(
    Guid Id,
    Guid AccountId,
    string Type,
    decimal Amount,
    string Currency,
    decimal BalanceAfter,
    string Description,
    string? Location,
    Guid? RelatedEntityId,
    bool IsInbound,
    DateTime OccurredAt)
{
    public static AccountMovementDto FromEntity(AccountMovement movement) => new(
        movement.Id,
        movement.AccountId,
        movement.Type.ToString(),
        movement.Amount.Amount,
        movement.Amount.Currency,
        movement.BalanceAfter.Amount,
        movement.Description,
        movement.Location,
        movement.RelatedEntityId,
        movement.IsInbound,
        movement.OccurredAt);
}