using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
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

    public Money NetPerDeposit =>
        _paymentDays.Count == 0
            ? NetMonthly
            : new Money(NetMonthly.Amount / _paymentDays.Count, GrossMonthlySalary.Currency);

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
        SetPaymentDays(paymentDays);
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