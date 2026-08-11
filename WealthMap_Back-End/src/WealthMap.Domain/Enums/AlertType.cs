namespace WealthMap.Domain.Enums;

public enum AlertType
{
    CardPaymentDueSoon = 1,
    HighDebtRatio = 2,
    OverspendingVsIncome = 3,
    GoalBehindSchedule = 4,
    InsufficientBalanceForCardPayment = 5,
    DebtPaymentDueSoon = 6,
    InstallmentDueSoon = 7,
    GoalReached = 8,

    /// <summary>
    /// Split from <see cref="GoalBehindSchedule"/>, which was raised for both a
    /// goal falling behind and a goal whose deadline had passed. They say
    /// different things, so a client rendering by type could not tell them apart.
    /// Appended rather than inserted: the value is persisted on notifications.
    /// </summary>
    GoalDeadlinePassed = 9
}
