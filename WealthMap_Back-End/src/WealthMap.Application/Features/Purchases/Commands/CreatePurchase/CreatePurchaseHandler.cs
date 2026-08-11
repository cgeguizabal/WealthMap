using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Purchases.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Purchases.Commands.CreatePurchase;

public class CreatePurchaseHandler : ICommandHandler<CreatePurchaseCommand, PurchaseDto>
{
    private readonly IPurchaseRepository _purchases;
    private readonly IAccountRepository _accounts;
    private readonly ICreditCardRepository _cards;
    private readonly IStoreRepository _stores;
    private readonly IAccountMovementRepository _movements;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaseHandler(
        IPurchaseRepository purchases,
        IAccountRepository accounts,
        ICreditCardRepository cards,
        IStoreRepository stores,
        IAccountMovementRepository movements,
        IUnitOfWork unitOfWork)
    {
        _purchases = purchases;
        _accounts = accounts;
        _cards = cards;
        _stores = stores;
        _movements = movements;
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseDto> Handle(CreatePurchaseCommand request, CancellationToken ct)
    {
        var method = (PaymentMethod)request.PaymentMethod;

        // The store is fetched to prove it exists; its name is kept so the response
        // can name it too, rather than returning a bare id the caller must resolve.
        string? storeName = null;

        if (request.StoreId is not null)
        {
            var store = await _stores.GetByIdAsync(request.StoreId.Value, ct)
                ?? throw new NotFoundException("Store", request.StoreId.Value);

            storeName = store.Name;
        }

        var occurredAt = NormalizeToUtc(request.OccurredAt) ?? DateTime.UtcNow;

        return method switch
        {
            PaymentMethod.DebitAccount => await HandleDebit(request, occurredAt, storeName, ct),
            PaymentMethod.CreditCard => await HandleCredit(request, occurredAt, storeName, ct),
            _ => await HandleCash(request, occurredAt, storeName, ct)
        };
    }

    private async Task<PurchaseDto> HandleDebit(
        CreatePurchaseCommand request, DateTime occurredAt, string? storeName, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.AccountId!.Value, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.AccountId.Value);

        var amount = new Money(request.Amount, account.Balance.Currency);

        var purchase = new Purchase(
            request.UserId, request.ProductName, amount, occurredAt, request.StoreId,
            request.Category, PaymentMethod.DebitAccount, account.Id, null, request.Notes);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            account.Withdraw(amount);

            var movement = new AccountMovement(
                account.Id,
                request.UserId,
                MovementType.Purchase,
                amount,
                account.Balance,
                $"Purchase: {purchase.ProductName}",
                occurredAt,
                relatedEntityId: purchase.Id);

            await _purchases.AddAsync(purchase, ct);
            await _movements.AddAsync(movement, ct);
        }, ct);

        return PurchaseDto.FromEntity(purchase, storeName);
    }

    private async Task<PurchaseDto> HandleCredit(
        CreatePurchaseCommand request, DateTime occurredAt, string? storeName, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.CreditCardId!.Value, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.CreditCardId.Value);

        var amount = new Money(request.Amount, card.CreditLimit.Currency);

        var purchase = new Purchase(
            request.UserId, request.ProductName, amount, occurredAt, request.StoreId,
            request.Category, PaymentMethod.CreditCard, null, card.Id, request.Notes);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            card.Charge(amount);
            await _purchases.AddAsync(purchase, ct);
        }, ct);

        return PurchaseDto.FromEntity(purchase, storeName);
    }

    private async Task<PurchaseDto> HandleCash(
        CreatePurchaseCommand request, DateTime occurredAt, string? storeName, CancellationToken ct)
    {
        // Cash has no instrument to borrow a currency from — the validator
        // guarantees one was sent explicitly.
        var amount = new Money(request.Amount, request.Currency!);

        var purchase = new Purchase(
            request.UserId, request.ProductName, amount, occurredAt, request.StoreId,
            request.Category, PaymentMethod.Cash, null, null, request.Notes);

        await _purchases.AddAsync(purchase, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return PurchaseDto.FromEntity(purchase, storeName);
    }

    private static DateTime? NormalizeToUtc(DateTime? value) => value switch
    {
        null => null,
        { Kind: DateTimeKind.Utc } utc => utc,
        { Kind: DateTimeKind.Unspecified } unspecified => DateTime.SpecifyKind(unspecified, DateTimeKind.Utc),
        _ => value.Value.ToUniversalTime()
    };
}
