using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Common.Services;

/// <summary>
/// Puts a purchase's money effects on, and takes them off again.
/// </summary>
/// <remarks>
/// A purchase is not a row. Depending on its method it withdrew from an account
/// and wrote a movement, or charged a card, or did neither. Correcting one means
/// undoing exactly what it did — and an edit is a reversal followed by a fresh
/// application, which is why both live here rather than being written twice.
/// </remarks>
public class PurchaseEffects
{
    private readonly IAccountRepository _accounts;
    private readonly ICreditCardRepository _cards;
    private readonly IAccountMovementRepository _movements;

    public PurchaseEffects(
        IAccountRepository accounts,
        ICreditCardRepository cards,
        IAccountMovementRepository movements)
    {
        _accounts = accounts;
        _cards = cards;
        _movements = movements;
    }

    /// <summary>
    /// Undoes what a purchase did: restores the account or card, removes the
    /// movement it wrote, and repairs the running balance of everything after it.
    /// </summary>
    public async Task ReverseAsync(Purchase purchase, CancellationToken ct)
    {
        if (purchase.PaymentMethod == PaymentMethod.CreditCard)
        {
            var card = await _cards.GetByIdForUserAsync(purchase.CreditCardId!.Value, purchase.UserId, ct)
                ?? throw new NotFoundException("CreditCard", purchase.CreditCardId.Value);

            // Throws when the balance has already been paid below this charge —
            // deliberately, rather than driving used credit negative.
            card.ReverseCharge(purchase.Amount);
            return;
        }

        if (purchase.PaymentMethod != PaymentMethod.DebitAccount) return;   // Cash moved nothing.

        var account = await _accounts.GetByIdForUserAsync(purchase.AccountId!.Value, purchase.UserId, ct)
            ?? throw new NotFoundException("Account", purchase.AccountId.Value);

        account.Deposit(purchase.Amount);

        var movement = await _movements.GetByRelatedEntityAsync(purchase.Id, purchase.UserId, ct);

        if (movement is null) return;

        // Every later movement recorded a running balance that assumed this one
        // happened. Removing it without rebasing them would leave the history
        // visibly failing to add up.
        var later = await _movements.GetForAccountAfterAsync(
            account.Id, purchase.UserId, movement.OccurredAt, ct);

        foreach (var subsequent in later)
            subsequent.RebaseBalanceAfter(purchase.Amount);

        _movements.Remove(movement);
    }

    /// <summary>
    /// Applies a purchase to the instrument it names, writing a movement when one
    /// is owed. Mirrors what creating it does.
    /// </summary>
    public async Task ApplyAsync(Purchase purchase, CancellationToken ct)
    {
        if (purchase.PaymentMethod == PaymentMethod.CreditCard)
        {
            var card = await _cards.GetByIdForUserAsync(purchase.CreditCardId!.Value, purchase.UserId, ct)
                ?? throw new NotFoundException("CreditCard", purchase.CreditCardId.Value);

            card.Charge(purchase.Amount);
            return;
        }

        if (purchase.PaymentMethod != PaymentMethod.Cash)
        {
            var account = await _accounts.GetByIdForUserAsync(purchase.AccountId!.Value, purchase.UserId, ct)
                ?? throw new NotFoundException("Account", purchase.AccountId.Value);

            account.Withdraw(purchase.Amount);

            await _movements.AddAsync(new AccountMovement(
                account.Id,
                purchase.UserId,
                MovementType.Purchase,
                purchase.Amount,
                account.Balance,
                $"Purchase: {purchase.ProductName}",
                purchase.OccurredAt,
                relatedEntityId: purchase.Id), ct);
        }
    }

    /// <summary>
    /// The currency the amount must be expressed in, which the instrument decides.
    /// </summary>
    /// <remarks>
    /// A correction can move a purchase between a dollar card and a peso account,
    /// and the amount has to follow. Reading it from the instrument rather than
    /// keeping the old currency is what stops <c>Money</c> throwing later.
    /// </remarks>
    public async Task<string> CurrencyForAsync(
        PaymentMethod method, Guid? accountId, Guid? creditCardId, Guid userId,
        string? fallback, CancellationToken ct)
    {
        if (method == PaymentMethod.CreditCard)
        {
            var card = await _cards.GetByIdForUserAsync(creditCardId!.Value, userId, ct)
                ?? throw new NotFoundException("CreditCard", creditCardId.Value);

            return card.CreditLimit.Currency;
        }

        if (method == PaymentMethod.DebitAccount)
        {
            var account = await _accounts.GetByIdForUserAsync(accountId!.Value, userId, ct)
                ?? throw new NotFoundException("Account", accountId.Value);

            return account.Balance.Currency;
        }

        // Cash has no instrument to inherit from; the caller supplies it. The
        // validator catches this first — the throw is the backstop, not the path.
        return !string.IsNullOrWhiteSpace(fallback)
            ? fallback
            : throw new DomainException("Currency is required for a cash purchase.");
    }
}
