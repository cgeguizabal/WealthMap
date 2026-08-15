using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// Interest-free installment purchase (tasa 0). The card is charged the FULL
/// price at creation — that is how available credit really behaves — and each
/// installment payment then pays the card back. Child rows are generated up
/// front; the last one absorbs any rounding remainder.
/// </summary>
public class InstallmentPurchase : BaseEntity
{
    private readonly List<InstallmentPayment> _payments = [];

    public Guid UserId { get; private set; }
    public string ProductName { get; private set; }
    public Money TotalPrice { get; private set; }
    public Guid? StoreId { get; private set; }
    public Guid CreditCardId { get; private set; }
    public int MonthsCount { get; private set; }
    public DateOnly PurchasedAt { get; private set; }

    public IReadOnlyCollection<InstallmentPayment> Payments => _payments.AsReadOnly();

    public Money MonthlyPayment =>
        _payments.Count == 0 ? TotalPrice : _payments[0].Amount;

    public Money RemainingBalance => _payments
        .Where(p => !p.IsPaid)
        .Aggregate(Money.Zero(TotalPrice.Currency), (sum, p) => sum + p.Amount);

    public int RemainingMonths => _payments.Count(p => !p.IsPaid);

    public DateOnly EndDate =>
        _payments.Count == 0 ? PurchasedAt : _payments.Max(p => p.DueDate);

    public bool IsCompleted => _payments.All(p => p.IsPaid);

    private InstallmentPurchase()
    {
        ProductName = null!;
    }

    public InstallmentPurchase(
        Guid userId,
        string productName,
        Money totalPrice,
        Guid? storeId,
        Guid creditCardId,
        int monthsCount,
        DateOnly purchasedAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Installment purchase must belong to a user.");

        if (creditCardId == Guid.Empty)
            throw new DomainException("An installment purchase requires a credit card.");

        if (totalPrice.IsZero || totalPrice.IsNegative)
            throw new DomainException("Total price must be greater than zero.");

        if (monthsCount is < 1 or > 120)
            throw new DomainException("Months must be between 1 and 120.");

        UserId = userId;
        ProductName = ValidateText(productName);
        TotalPrice = totalPrice;
        StoreId = storeId;
        CreditCardId = creditCardId;
        MonthsCount = monthsCount;
        PurchasedAt = purchasedAt;

        GeneratePayments();
    }

    public InstallmentPayment NextUnpaid() =>
        _payments.Where(p => !p.IsPaid).OrderBy(p => p.Number).FirstOrDefault()
            ?? throw new DomainException($"'{ProductName}' is already fully paid.");

    public InstallmentPayment PayNextInstallment()
    {
        var installment = NextUnpaid();
        installment.MarkPaid();
        Touch();
        return installment;
    }

    /// <summary>
    /// Marks installments falling due on or before <paramref name="through"/> as
    /// paid, oldest first, for as long as <paramref name="available"/> covers each
    /// one in full. Returns those settled.
    /// </summary>
    /// <remarks>
    /// This is what a card payment does to a plan. The installment for the month is
    /// part of the card's statement, so paying that statement settles it — without
    /// this the plan would sit unchanged while the balance it belongs to went down,
    /// and the schedule would drift out of step with what is actually owed.
    ///
    /// Whole installments only. A payment covering half of one leaves it unpaid,
    /// because the schedule has no notion of a part-paid month and inventing one
    /// would make "how many months are left" unanswerable.
    ///
    /// Nothing beyond <paramref name="through"/> is touched even when the money
    /// would stretch further. Paying next year's installments early is a deliberate
    /// act with its own action, not something that should happen silently because a
    /// card payment was generous.
    /// </remarks>
    public IReadOnlyList<InstallmentPayment> SettleDueThrough(DateOnly through, Money available)
    {
        var settled = new List<InstallmentPayment>();
        var remaining = available;

        var due = _payments
            .Where(p => !p.IsPaid && p.DueDate <= through)
            .OrderBy(p => p.Number);

        foreach (var installment in due)
        {
            if (remaining.Amount < installment.Amount.Amount) break;

            installment.MarkPaid();
            remaining -= installment.Amount;
            settled.Add(installment);
        }

        if (settled.Count > 0) Touch();

        return settled;
    }

    private void GeneratePayments()
    {
        var baseAmount = new Money(TotalPrice.Amount / MonthsCount, TotalPrice.Currency);

        if (baseAmount.IsZero)
            throw new DomainException("Too many months for this amount: installments would be zero.");

        // Last installment absorbs the rounding remainder so the sum is exact.
        var lastAmount = new Money(
            TotalPrice.Amount - baseAmount.Amount * (MonthsCount - 1),
            TotalPrice.Currency);

        if (lastAmount.IsZero || lastAmount.IsNegative)
            throw new DomainException("Total does not split into this many installments.");

        for (var i = 1; i <= MonthsCount; i++)
        {
            _payments.Add(new InstallmentPayment(
                Id,
                i,
                i == MonthsCount ? lastAmount : baseAmount,
                PurchasedAt.AddMonths(i)));
        }
    }

    private static string ValidateText(string value) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new DomainException("Product name is required.");
}
