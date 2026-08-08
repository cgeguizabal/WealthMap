using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Debts.DTOs;

public record DebtPaymentResultDto(
    DebtDto Debt,
    AccountMovementDto? AccountMovement);
