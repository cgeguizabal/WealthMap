namespace WealthMap.Domain.Enums;

/// <summary>
/// Where the money came from. External means cash or a third party paid — the debt
/// shrinks without any tracked account moving.
/// </summary>
public enum PaymentSourceType
{
    Account = 1,
    External = 2
}