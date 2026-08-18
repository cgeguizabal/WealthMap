namespace WealthMap.Domain.Enums;

public enum MovementType
{
    SalaryDeposit = 1,
    Deposit = 2,
    Bonus = 3,
    TransferIn = 4,
    TransferOut = 5,
    Purchase = 6,
    Payment = 7,
    AtmWithdrawal = 8,

    /// <summary>
    /// A client paying for freelance work. Its own type rather than a plain
    /// Deposit so the monthly report can separate irregular earned income from
    /// money the user simply moved in.
    /// </summary>
    FreelanceIncome = 9
}