using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Commands.CreateInstallmentPurchase;

public record CreateInstallmentPurchaseCommand(
    Guid UserId,
    string ProductName,
    decimal TotalPrice,
    Guid? StoreId,
    Guid CreditCardId,
    int MonthsCount,
    DateOnly? PurchasedAt) : ICommand<InstallmentPurchaseDto>;
