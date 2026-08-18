using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.CreditCards.DTOs;

/// <param name="SettledInstallments">
/// Installments this payment cleared, because they were part of the statement it
/// covered. Reported so the client can say so rather than leaving the user to
/// notice a plan quietly advanced.
/// </param>
public record CardPaymentResultDto(
    CreditCardDto Card,
    AccountMovementDto? AccountMovement,
    IReadOnlyList<SettledInstallmentDto> SettledInstallments);

public record SettledInstallmentDto(
    Guid InstallmentPurchaseId,
    string ProductName,
    int Number,
    int MonthsCount,
    decimal Amount);
