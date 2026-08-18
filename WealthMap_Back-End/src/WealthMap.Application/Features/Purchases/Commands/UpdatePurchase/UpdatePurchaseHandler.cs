using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Purchases.DTOs;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Purchases.Commands.UpdatePurchase;

/// <summary>
/// Corrects a purchase that was recorded wrongly.
/// </summary>
/// <remarks>
/// Implemented as a reversal followed by a fresh application, rather than as an
/// adjustment by the difference. Working out a delta only holds while the method
/// and instrument stay the same, and the correction people most need is exactly
/// the one that breaks that — "it went on the other card". Undoing and redoing
/// handles every case with one path, and reuses the code creation already trusts.
/// </remarks>
public class UpdatePurchaseHandler : ICommandHandler<UpdatePurchaseCommand, PurchaseDto>
{
    private readonly IPurchaseRepository _purchases;
    private readonly IStoreRepository _stores;
    private readonly PurchaseEffects _effects;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePurchaseHandler(
        IPurchaseRepository purchases,
        IStoreRepository stores,
        PurchaseEffects effects,
        IUnitOfWork unitOfWork)
    {
        _purchases = purchases;
        _stores = stores;
        _effects = effects;
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseDto> Handle(UpdatePurchaseCommand request, CancellationToken ct)
    {
        var purchase = await _purchases.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Purchase", request.Id);

        var method = (PaymentMethod)request.PaymentMethod;

        // Proven to exist before anything moves, and its name kept so the response
        // can name it rather than returning a bare id.
        string? storeName = null;

        if (request.StoreId is not null)
        {
            var store = await _stores.GetByIdAsync(request.StoreId.Value, ct)
                ?? throw new NotFoundException("Store", request.StoreId.Value);

            storeName = store.Name;
        }

        // The instrument decides the currency, and the instrument may be changing.
        var currency = await _effects.CurrencyForAsync(
            method, request.AccountId, request.CreditCardId, request.UserId, request.Currency, ct);

        var amount = new Money(request.Amount, currency);
        var occurredAt = NormalizeToUtc(request.OccurredAt) ?? purchase.OccurredAt;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Order matters. Reversing first frees the credit the old charge was
            // holding, so moving a purchase to a nearly-full card still fits when
            // it is the same card being corrected.
            await _effects.ReverseAsync(purchase, ct);

            purchase.Update(
                request.ProductName,
                amount,
                occurredAt,
                request.StoreId,
                request.Category,
                method,
                request.AccountId,
                request.CreditCardId,
                request.Notes);

            await _effects.ApplyAsync(purchase, ct);
        }, ct);

        return PurchaseDto.FromEntity(purchase, storeName);
    }

    private static DateTime? NormalizeToUtc(DateTime? value) => value switch
    {
        null => null,
        { Kind: DateTimeKind.Utc } => value,
        { Kind: DateTimeKind.Local } => value.Value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
    };
}
