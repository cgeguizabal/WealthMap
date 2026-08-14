using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Models;

namespace WealthMap.Application.Common.Services;

/// <summary>
/// Loads one user's whole financial picture. Shared by the dashboard and the
/// alert rules so both answer from exactly the same numbers.
/// </summary>
public class FinancialSnapshotLoader
{
    private readonly IUserRepository _users;
    private readonly IAccountRepository _accounts;
    private readonly ICreditCardRepository _cards;
    private readonly IDebtRepository _debts;
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly ISavingsGoalRepository _savingsGoals;
    private readonly IProductGoalRepository _productGoals;
    private readonly IJobRepository _jobs;
    private readonly IAdditionalIncomeRepository _incomes;
    private readonly IPurchaseRepository _purchases;
    private readonly IAccountMovementRepository _movements;

    public FinancialSnapshotLoader(
        IUserRepository users,
        IAccountRepository accounts,
        ICreditCardRepository cards,
        IDebtRepository debts,
        IInstallmentPurchaseRepository installments,
        ISavingsGoalRepository savingsGoals,
        IProductGoalRepository productGoals,
        IJobRepository jobs,
        IAdditionalIncomeRepository incomes,
        IPurchaseRepository purchases,
        IAccountMovementRepository movements)
    {
        _users = users;
        _accounts = accounts;
        _cards = cards;
        _debts = debts;
        _installments = installments;
        _savingsGoals = savingsGoals;
        _productGoals = productGoals;
        _jobs = jobs;
        _incomes = incomes;
        _purchases = purchases;
        _movements = movements;
    }

    public async Task<FinancialSnapshot> LoadAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var accounts = await _accounts.GetAllForUserAsync(userId, ct: ct);
        var cards = await _cards.GetAllForUserAsync(userId, ct: ct);
        var debts = await _debts.GetAllForUserAsync(userId, ct: ct);
        var installments = await _installments.GetAllForUserAsync(userId, ct: ct);
        var savingsGoals = await _savingsGoals.GetAllForUserAsync(userId, ct: ct);
        var productGoals = await _productGoals.GetAllForUserAsync(userId, ct: ct);
        var jobs = await _jobs.GetAllForUserAsync(userId, ct: ct);
        var incomes = await _incomes.GetAllForUserAsync(userId, ct: ct);
        var monthPurchases = await _purchases.GetForUserInMonthAsync(userId, today.Year, today.Month, ct);

        // From the first of the month, so deposits made this month lift the amount
        // still safe to spend. Movements before that are already reflected in the
        // account balances and would double-count if included.
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthMovements = await _movements.GetForUserFromAsync(userId, monthStart, ct);

        return new FinancialSnapshot(
            user.Currency,
            today,
            accounts,
            cards,
            debts,
            installments,
            savingsGoals,
            productGoals,
            jobs.FirstOrDefault(),
            incomes,
            monthPurchases,
            monthMovements);
    }
}
