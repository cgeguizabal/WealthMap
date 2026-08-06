namespace WealthMap.Application.Features.Accounts.DTOs;

public record TransferResultDto(
    AccountDto FromAccount,
    AccountDto ToAccount,
    decimal Amount,
    string Currency,
    DateTime OccurredAt);