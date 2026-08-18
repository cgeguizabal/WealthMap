using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.InstallmentPurchases.Commands.CreateInstallmentPurchase;

public class CreateInstallmentPurchaseHandler
    : ICommandHandler<CreateInstallmentPurchaseCommand, InstallmentPurchaseDto>
{
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly ICreditCardRepository _cards;
    private readonly IStoreRepository _stores;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InstallmentContextLoader _context;
    private readonly IUserClock _clock;

    public CreateInstallmentPurchaseHandler(
        IInstallmentPurchaseRepository installments,
        ICreditCardRepository cards,
        IStoreRepository stores,
        IUnitOfWork unitOfWork,
        InstallmentContextLoader context,
        IUserClock clock)
    {
        _context = context;
        _installments = installments;
        _cards = cards;
        _stores = stores;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<InstallmentPurchaseDto> Handle(
        CreateInstallmentPurchaseCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.CreditCardId, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.CreditCardId);

        if (request.StoreId is not null
            && await _stores.GetByIdAsync(request.StoreId.Value, ct) is null)
            throw new NotFoundException("Store", request.StoreId.Value);

        var totalPrice = new Money(request.TotalPrice, card.CreditLimit.Currency);

        var purchase = new InstallmentPurchase(
            request.UserId,
            request.ProductName,
            totalPrice,
            request.StoreId,
            card.Id,
            request.MonthsCount,
            request.PurchasedAt ?? _clock.Today);

        // Tasa 0 still consumes the full credit line on day one; the card's
        // limit guard rejects the plan if there isn't enough available credit.
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            card.Charge(totalPrice);
            await _installments.AddAsync(purchase, ct);
        }, ct);

        return await _context.ToDtoAsync(purchase, request.UserId, ct);
    }
}
