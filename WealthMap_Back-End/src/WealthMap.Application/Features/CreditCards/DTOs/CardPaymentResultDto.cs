using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.CreditCards.DTOs;

public record CardPaymentResultDto(
    CreditCardDto Card,
    AccountMovementDto? AccountMovement);