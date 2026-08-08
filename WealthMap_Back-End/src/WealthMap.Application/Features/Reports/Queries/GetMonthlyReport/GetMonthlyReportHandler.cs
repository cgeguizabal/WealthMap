using System.Globalization;
using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Reports.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Reports.Queries.GetMonthlyReport;

public class GetMonthlyReportHandler : IQueryHandler<GetMonthlyReportQuery, MonthlyReportDto>
{
    private const int TopExpenseCount = 5;

    private readonly IUserRepository _users;
    private readonly IAccountRepository _accounts;
    private readonly IAccountMovementRepository _movements;
    private readonly IPurchaseRepository _purchases;
    private readonly ICreditCardRepository _cards;
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly ISavingsGoalRepository _savingsGoals;
    private readonly IProductGoalRepository _productGoals;
    private readonly IJobRepository _jobs;

    public GetMonthlyReportHandler(
        IUserRepository users,
        IAccountRepository accounts,
        IAccountMovementRepository movements,
        IPurchaseRepository purchases,
        ICreditCardRepository cards,
        IInstallmentPurchaseRepository installments,
        ISavingsGoalRepository savingsGoals,
        IProductGoalRepository productGoals,
        IJobRepository jobs)
    {
        _users = users;
        _accounts = accounts;
        _movements = movements;
        _purchases = purchases;
        _cards = cards;
        _installments = installments;
        _savingsGoals = savingsGoals;
        _productGoals = productGoals;
        _jobs = jobs;
    }

    public async Task<MonthlyReportDto> Handle(GetMonthlyReportQuery request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        var monthStart = DateTime.SpecifyKind(
            DateTime.ParseExact(request.Month, "yyyy-MM", CultureInfo.InvariantCulture),
            DateTimeKind.Utc);

        var monthEnd = monthStart.AddMonths(1);
        var currency = user.Currency;

        // Anything opened after the period ended did not exist during it.
        var accounts = (await _accounts.GetAllForUserAsync(request.UserId, ct))
            .Where(a => a.Balance.Currency == currency && a.CreatedAt < monthEnd)
            .ToList();

        // Everything from the month's first day onward: enough to rewind today's
        // balances back to the month's opening and closing positions.
        var movementsFromStart = await _movements.GetForUserFromAsync(request.UserId, monthStart, ct);

        var monthMovements = movementsFromStart
            .Where(m => m.OccurredAt < monthEnd && m.Amount.Currency == currency)
            .ToList();

        var purchases = (await _purchases.GetForUserInMonthAsync(request.UserId, monthStart.Year, monthStart.Month, ct))
            .Where(p => p.Amount.Currency == currency)
            .ToList();

        var cards = (await _cards.GetAllForUserAsync(request.UserId, ct))
            .Where(c => c.CreditLimit.Currency == currency && c.CreatedAt < monthEnd)
            .ToList();

        var installments = (await _installments.GetAllForUserAsync(request.UserId, ct))
            .Where(i => i.TotalPrice.Currency == currency)
            .ToList();

        var jobs = await _jobs.GetAllForUserAsync(request.UserId, ct);
        var job = jobs.FirstOrDefault(j => j.GrossMonthlySalary.Currency == currency);

        var income = BuildIncome(monthMovements, job);
        var spending = BuildSpending(purchases, monthMovements);
        var accountSummaries = BuildAccounts(accounts, movementsFromStart, monthEnd);
        var cardSummaries = BuildCards(cards, purchases, installments, monthMovements, monthStart, monthEnd);
        var goals = await BuildGoals(request.UserId, currency, ct);

        return new MonthlyReportDto(
            request.Month,
            currency,
            DateOnly.FromDateTime(monthStart),
            DateOnly.FromDateTime(monthEnd.AddDays(-1)),
            user.FullName,
            income,
            spending,
            accountSummaries,
            cardSummaries,
            goals,
            income.Total - spending.TotalPurchases,
            DateTime.UtcNow);
    }

    private static IncomeSectionDto BuildIncome(IReadOnlyList<AccountMovement> monthMovements, Job? job)
    {
        // TransferIn is money moving between the user's own accounts, not income.
        var incomeTypes = new[] { MovementType.SalaryDeposit, MovementType.Deposit, MovementType.Bonus };

        var lines = monthMovements
            .Where(m => incomeTypes.Contains(m.Type))
            .GroupBy(m => m.Type)
            .Select(g => new IncomeLineDto(g.Key.ToString(), g.Sum(m => m.Amount.Amount), g.Count()))
            .OrderByDescending(l => l.Total)
            .ToList();

        return new IncomeSectionDto(
            lines.Sum(l => l.Total),
            lines,
            job?.NetMonthly.Amount ?? 0m);
    }

    private static SpendingSectionDto BuildSpending(
        IReadOnlyList<Purchase> purchases, IReadOnlyList<AccountMovement> monthMovements)
    {
        var totalPurchases = purchases.Sum(p => p.Amount.Amount);

        var byCategory = purchases
            .GroupBy(p => p.Category, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var total = g.Sum(p => p.Amount.Amount);
                return new CategorySpendDto(
                    g.Key,
                    total,
                    g.Count(),
                    totalPurchases == 0 ? 0 : decimal.Round(total / totalPurchases * 100m, 2));
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        var top = purchases
            .OrderByDescending(p => p.Amount.Amount)
            .Take(TopExpenseCount)
            .Select(p => new ExpenseLineDto(
                p.ProductName,
                p.Category,
                p.Amount.Amount,
                DateOnly.FromDateTime(p.OccurredAt),
                p.PaymentMethod.ToString()))
            .ToList();

        // Cash that left tracking. Kept out of the net result: whatever it buys is
        // already counted as a Cash purchase, so adding both would double count.
        var cashWithdrawn = monthMovements
            .Where(m => m.Type == MovementType.AtmWithdrawal)
            .Sum(m => m.Amount.Amount);

        return new SpendingSectionDto(totalPurchases, cashWithdrawn, byCategory, top);
    }

    private static List<AccountSummaryDto> BuildAccounts(
        IReadOnlyList<Account> accounts,
        IReadOnlyList<AccountMovement> movementsFromStart,
        DateTime monthEnd)
    {
        var summaries = new List<AccountSummaryDto>();

        foreach (var account in accounts)
        {
            var mine = movementsFromStart.Where(m => m.AccountId == account.Id).ToList();
            var inMonth = mine.Where(m => m.OccurredAt < monthEnd).ToList();

            var current = account.Balance.Amount;

            // Rewind: today's balance minus everything that happened from the month
            // start onward is the opening position; minus only what happened after
            // the month ended is the closing position.
            var opening = current - mine.Sum(Signed);
            var closing = current - mine.Where(m => m.OccurredAt >= monthEnd).Sum(Signed);

            summaries.Add(new AccountSummaryDto(
                account.Id,
                account.Name,
                account.Type.ToString(),
                opening,
                closing,
                inMonth.Where(m => m.IsInbound).Sum(m => m.Amount.Amount),
                inMonth.Where(m => !m.IsInbound).Sum(m => m.Amount.Amount),
                inMonth.Count));
        }

        return summaries;

        static decimal Signed(AccountMovement m) => m.IsInbound ? m.Amount.Amount : -m.Amount.Amount;
    }

    private static List<CardSummaryDto> BuildCards(
        IReadOnlyList<CreditCard> cards,
        IReadOnlyList<Purchase> purchases,
        IReadOnlyList<InstallmentPurchase> installments,
        IReadOnlyList<AccountMovement> monthMovements,
        DateTime monthStart,
        DateTime monthEnd)
    {
        var monthStartDate = DateOnly.FromDateTime(monthStart);
        var monthEndDate = DateOnly.FromDateTime(monthEnd);

        return cards.Select(card =>
        {
            var charged = purchases
                .Where(p => p.CreditCardId == card.Id)
                .Sum(p => p.Amount.Amount);

            // An installment plan charges the card its full price on the purchase date.
            charged += installments
                .Where(i => i.CreditCardId == card.Id
                            && i.PurchasedAt >= monthStartDate
                            && i.PurchasedAt < monthEndDate)
                .Sum(i => i.TotalPrice.Amount);

            // Only account-sourced payments leave a movement; external ones are invisible here.
            var paid = monthMovements
                .Where(m => m.Type == MovementType.Payment && m.RelatedEntityId == card.Id)
                .Sum(m => m.Amount.Amount);

            return new CardSummaryDto(
                card.Id,
                card.CardName,
                card.CreditLimit.Amount,
                card.UsedCredit.Amount,
                card.AvailableCredit.Amount,
                charged,
                paid,
                card.PaymentDueDay);
        }).ToList();
    }

    private async Task<List<GoalSummaryDto>> BuildGoals(Guid userId, string currency, CancellationToken ct)
    {
        var savings = (await _savingsGoals.GetAllForUserAsync(userId, ct))
            .Where(g => g.TargetAmount.Currency == currency)
            .Select(g => new GoalSummaryDto(
                "Savings", g.Name, g.TargetAmount.Amount, g.CurrentAmount.Amount,
                g.ProgressPercentage, g.Status.ToString()));

        var products = (await _productGoals.GetAllForUserAsync(userId, ct))
            .Where(g => g.TargetAmount.Currency == currency)
            .Select(g => new GoalSummaryDto(
                "Product", g.Name, g.TargetAmount.Amount, g.CurrentAmount.Amount,
                g.ProgressPercentage, g.Status.ToString()));

        return savings.Concat(products).OrderBy(g => g.Name).ToList();
    }
}
