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
    private readonly IUserClock _clock;

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
        IUserClock clock)
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
        _clock = clock;
    }

    public async Task<FinancialSnapshot> LoadAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        // The caller's date, not UTC. This feeds the alert thresholds and the
        // liquidity projection, and both ask when the next cutoff or due day
        // falls — a question that answers next month if the server's date has
        // rolled over and the user's has not.
        var today = _clock.Today;

        var accounts = await _accounts.GetAllForUserAsync(userId, ct: ct);
        var cards = await _cards.GetAllForUserAsync(userId, ct: ct);
        var debts = await _debts.GetAllForUserAsync(userId, ct: ct);
        var installments = await _installments.GetAllForUserAsync(userId, ct: ct);
        var savingsGoals = await _savingsGoals.GetAllForUserAsync(userId, ct: ct);
        var productGoals = await _productGoals.GetAllForUserAsync(userId, ct: ct);
        var jobs = await _jobs.GetAllForUserAsync(userId, ct: ct);
        var incomes = await _incomes.GetAllForUserAsync(userId, ct: ct);
        var monthPurchases = await _purchases.GetForUserInMonthAsync(userId, today.Year, today.Month, ct);


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
            monthPurchases);
    }
}
