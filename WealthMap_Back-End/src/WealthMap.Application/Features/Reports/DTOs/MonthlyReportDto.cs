namespace WealthMap.Application.Features.Reports.DTOs;

public record IncomeLineDto(string Type, decimal Total, int Count);

public record IncomeSectionDto(
    decimal Total,
    IReadOnlyList<IncomeLineDto> Lines,
    decimal ExpectedSalaryNet);

public record CategorySpendDto(string Category, decimal Total, int Count, decimal SharePercentage);

public record ExpenseLineDto(
    string ProductName,
    string Category,
    decimal Amount,
    /// <summary>The full UTC instant, so the hour survives as far as the report.</summary>
    DateTime OccurredAt,
    string PaymentMethod);

public record SpendingSectionDto(
    decimal TotalPurchases,
    decimal TotalCashWithdrawn,
    IReadOnlyList<CategorySpendDto> ByCategory,
    IReadOnlyList<ExpenseLineDto> TopExpenses);

public record AccountSummaryDto(
    Guid AccountId,
    string Name,
    string Type,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal TotalIn,
    decimal TotalOut,
    int MovementCount);

public record CardSummaryDto(
    Guid CardId,
    string CardName,
    decimal CreditLimit,
    decimal UsedCredit,
    decimal AvailableCredit,
    decimal ChargedThisMonth,
    decimal PaidThisMonth,
    int PaymentDueDay);

public record GoalSummaryDto(
    string Kind,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal ProgressPercentage,
    string Status);

public record MonthlyReportDto(
    string Month,
    string Currency,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string UserFullName,
    IncomeSectionDto Income,
    SpendingSectionDto Spending,
    IReadOnlyList<AccountSummaryDto> Accounts,
    IReadOnlyList<CardSummaryDto> Cards,
    IReadOnlyList<GoalSummaryDto> Goals,
    decimal NetResult,
    DateTime GeneratedAt);
