using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

public class InstallmentPayment : BaseEntity
{
    public Guid InstallmentPurchaseId { get; private set; }
    public int Number { get; private set; }
    public Money Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private InstallmentPayment() { }

    internal InstallmentPayment(Guid installmentPurchaseId, int number, Money amount, DateOnly dueDate)
    {
        if (number < 1)
            throw new DomainException("Installment number must be 1 or greater.");

        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Installment amount must be greater than zero.");

        InstallmentPurchaseId = installmentPurchaseId;
        Number = number;
        Amount = amount;
        DueDate = dueDate;
        IsPaid = false;
    }

    internal void MarkPaid()
    {
        if (IsPaid)
            throw new DomainException($"Installment {Number} is already paid.");

        IsPaid = true;
        PaidAt = DateTime.UtcNow;
        Touch();
    }
}
