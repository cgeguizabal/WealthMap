using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// Aggregate root for salary: gross is a stored fact, net figures are computed
/// from the user-declared deductions. One job per user.
/// </summary>
public class Job : BaseEntity
{
    private readonly List<JobPaymentDay> _paymentDays = [];
    private readonly List<Deduction> _deductions = [];

    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string Employer { get; private set; }
    public Money GrossMonthlySalary { get; private set; }
    public Guid DepositAccountId { get; private set; }

    /// <summary>
    /// The first date salary may be posted automatically. Payment days that fell
    /// before it are never posted, so adding a job today cannot backfill months of
    /// deposits into the account and invent a balance that never existed.
    /// </summary>
    public DateOnly SalaryPostingStartsOn { get; private set; }

    public IReadOnlyCollection<JobPaymentDay> PaymentDays => _paymentDays.AsReadOnly();
    public IReadOnlyCollection<Deduction> Deductions => _deductions.AsReadOnly();

    public Money NetMonthly
    {
        get
        {
            var gross = GrossMonthlySalary.Amount;
            var fixedTotal = _deductions.Where(d => d.Type == DeductionType.Fixed).Sum(d => d.Value);
            var percentTotal = _deductions.Where(d => d.Type == DeductionType.Percentage).Sum(d => d.Value);

            return new Money(gross - fixedTotal - gross * percentTotal / 100m, GrossMonthlySalary.Currency);
        }
    }

    /// <summary>
    /// The even share of net pay per payday — the headline figure. Deductions are
    /// monthly and each payday carries an equal part of them, so a 10% deduction
    /// paid twice a month takes 5% at each payday, not 10% at both.
    /// </summary>
    /// <remarks>
    /// Rounded, so it is the nominal share rather than what is literally paid on
    /// the final payday. Use <see cref="NetForPayday"/> to pay money.
    /// </remarks>
    public Money NetPerDeposit =>
        _paymentDays.Count == 0
            ? NetMonthly
            : new Money(NetMonthly.Amount / _paymentDays.Count, GrossMonthlySalary.Currency);

    /// <summary>
    /// What is actually deposited on one payday, with the rounding remainder for
    /// the month settled on its last payday.
    /// </summary>
    /// <remarks>
    /// An even share alone does not add up: 1000 across three paydays rounds to
    /// 333.33 each and pays 999.99, losing a cent every month for as long as the
    /// job exists. The last payday absorbs the difference so the deposits sum to
    /// <see cref="NetMonthly"/> exactly.
    /// </remarks>
    public Money NetForPayday(DateOnly payday)
    {
        var paydays = PaydaysInMonth(payday.Year, payday.Month);
        var index = paydays.ToList().IndexOf(payday);

        if (paydays.Count == 0 || index < 0)
            return NetPerDeposit;

        var currency = GrossMonthlySalary.Currency;
        var monthly = NetMonthly.Amount;
        var share = decimal.Round(monthly / paydays.Count, 2, MidpointRounding.ToEven);

        return index == paydays.Count - 1
            ? new Money(monthly - share * (paydays.Count - 1), currency)
            : new Money(share, currency);
    }

    /// <summary>
    /// Every payday in the month, clamped and de-duplicated, oldest first.
    /// Unlike <see cref="ScheduledDatesBetween"/> this ignores the posting anchor,
    /// because splitting a month's pay depends on how many paydays the month has,
    /// not on which of them are still owed.
    /// </summary>
    public IReadOnlyList<DateOnly> PaydaysInMonth(int year, int month) =>
        _paymentDays
            .Select(d => PaymentSchedule.ClampToMonth(year, month, d.DayOfMonth))
            .Distinct()
            .Order()
            .ToList();

    private Job()
    {
        Title = null!;
        Employer = null!;
    }

    public Job(
        Guid userId,
        string title,
        string employer,
        Money grossMonthlySalary,
        Guid depositAccountId,
        IEnumerable<int> paymentDays)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Job must belong to a user.");

        UserId = userId;
        Title = ValidateText(title, "Job title");
        Employer = ValidateText(employer, "Employer");
        GrossMonthlySalary = ValidateGross(grossMonthlySalary);
        DepositAccountId = ValidateAccount(depositAccountId);
        SalaryPostingStartsOn = DateOnly.FromDateTime(DateTime.UtcNow);
        SetPaymentDays(paymentDays);
    }

    /// <summary>
    /// Every date salary is due within the inclusive range, oldest first.
    /// </summary>
    /// <remarks>
    /// A payment day of 31 is clamped to the last day of shorter months, so a
    /// month-end salary lands on 30 June and 28 February rather than being skipped.
    /// Dates before <see cref="SalaryPostingStartsOn"/> are excluded.
    /// </remarks>
    public IReadOnlyList<DateOnly> ScheduledDatesBetween(DateOnly from, DateOnly to)
    {
        if (to < from || _paymentDays.Count == 0)
            return [];

        var effectiveFrom = from < SalaryPostingStartsOn ? SalaryPostingStartsOn : from;
        if (to < effectiveFrom)
            return [];

        var dates = new List<DateOnly>();
        var month = new DateOnly(effectiveFrom.Year, effectiveFrom.Month, 1);
        var lastMonth = new DateOnly(to.Year, to.Month, 1);

        while (month <= lastMonth)
        {
            // PaydaysInMonth already clamps and de-duplicates: two payment days can
            // land on the same date in a short month (30 and 31 in June), and that
            // is one payday, not two.
            dates.AddRange(PaydaysInMonth(month.Year, month.Month)
                .Where(date => date >= effectiveFrom && date <= to));

            month = month.AddMonths(1);
        }

        return dates;
    }

    public void UpdateDetails(string title, string employer, Money grossMonthlySalary, Guid depositAccountId)
    {
        if (grossMonthlySalary.Currency != GrossMonthlySalary.Currency)
            throw new DomainException("Cannot change the currency of an existing job.");

        Title = ValidateText(title, "Job title");
        Employer = ValidateText(employer, "Employer");
        GrossMonthlySalary = ValidateGross(grossMonthlySalary);
        DepositAccountId = ValidateAccount(depositAccountId);
        EnsureDeductionsFitGross();
        Touch();
    }

    public void SetPaymentDays(IEnumerable<int> days)
    {
        var distinct = days.Distinct().ToArray();

        if (distinct.Length is < 1 or > 3)
            throw new DomainException("A job must have between 1 and 3 payment days per month.");

        _paymentDays.Clear();
        _paymentDays.AddRange(distinct.Select(d => new JobPaymentDay(Id, d)));
        Touch();
    }

    public Deduction AddDeduction(string name, DeductionType type, decimal value)
    {
        var deduction = new Deduction(Id, name, type, value);
        _deductions.Add(deduction);
        EnsureDeductionsFitGross();
        Touch();
        return deduction;
    }

    public void UpdateDeduction(Guid deductionId, string name, DeductionType type, decimal value)
    {
        FindDeduction(deductionId).Update(name, type, value);
        EnsureDeductionsFitGross();
        Touch();
    }

    public void RemoveDeduction(Guid deductionId)
    {
        _deductions.Remove(FindDeduction(deductionId));
        Touch();
    }

    public bool HasDeduction(Guid deductionId) => _deductions.Any(d => d.Id == deductionId);

    private Deduction FindDeduction(Guid deductionId) =>
        _deductions.FirstOrDefault(d => d.Id == deductionId)
            ?? throw new DomainException("Deduction not found on this job.");

    private void EnsureDeductionsFitGross()
    {
        if (NetMonthly.IsNegative)
            throw new DomainException(
                $"Deductions exceed the gross salary ({GrossMonthlySalary}). Net cannot be negative.");
    }

    private static string ValidateText(string value, string field) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new DomainException($"{field} is required.");

    private static Money ValidateGross(Money gross) =>
        gross.IsZero || gross.IsNegative
            ? throw new DomainException("Gross monthly salary must be greater than zero.")
            : gross;

    private static Guid ValidateAccount(Guid depositAccountId) =>
        depositAccountId == Guid.Empty
            ? throw new DomainException("A deposit account is required.")
            : depositAccountId;
}