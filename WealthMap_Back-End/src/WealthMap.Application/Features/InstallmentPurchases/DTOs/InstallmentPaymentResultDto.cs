using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.DTOs;

public record InstallmentPaymentResultDto(
    InstallmentPurchaseDto Purchase,
    AccountMovementDto? AccountMovement);
