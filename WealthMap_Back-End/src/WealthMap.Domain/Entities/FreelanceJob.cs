using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

/// <summary>
/// One piece of freelance work: agreed with a client, delivered at some point,
/// paid at some other point, on no schedule at all.
/// </summary>
/// <remarks>
/// Deliberately not an <see cref="AdditionalIncome"/>, which is recurring money
/// on a frequency, and not a <see cref="Job"/>, which pays on fixed days of the
/// month. Freelance work has neither property: the dates are whatever they turn
/// out to be, and the user is the only one who knows when they happened. So
/// nothing here is posted automatically — every state change is something a
/// person recorded.
///
/// Status is computed from the dates rather than stored, following the same rule
/// as the rest of the domain: facts are stored, conclusions are computed. A
/// stored status could disagree with the dates that produced it.
///
/// The amount actually paid is kept separately from the amount agreed. Clients
/// pay late, short, or with a bonus, and a model that assumed they matched would
/// force the user to falsify one of them.
/// </remarks>
public class FreelanceJob : BaseEntity
{
    public Guid UserId { get; private set; }

    /// <summary>What the work is. Encrypted at rest — it names a client's project.</summary>
    public string Title { get; private set; }

    /// <summary>Who it is for. Optional, and encrypted for the same reason.</summary>
    public string? Client { get; private set; }

    public Money AgreedAmount { get; private set; }

    /// <summary>
    /// What actually arrived. Zero until payment is recorded — <see cref="PaidOn"/>
    /// is what says whether it has been, not this.
    /// </summary>
    public Money AmountPaid { get; private set; }

    /// <summary>When it was agreed to be finished, if a date was agreed at all.</summary>
    public DateOnly? DueOn { get; private set; }

    public DateOnly? DeliveredOn { get; private set; }
    public DateOnly? PaidOn { get; private set; }
    public DateOnly? CancelledOn { get; private set; }

    /// <summary>Where the payment landed. Null until paid.</summary>
    public Guid? DepositAccountId { get; private set; }

    public string? Notes { get; private set; }

    public FreelanceJobStatus Status =>
        CancelledOn is not null ? FreelanceJobStatus.Cancelled
        : PaidOn is not null ? FreelanceJobStatus.Paid
        : DeliveredOn is not null ? FreelanceJobStatus.Delivered
        : FreelanceJobStatus.InProgress;

    /// <summary>
    /// Still owed by the client. Zero once paid, whatever the agreed figure was.
    /// </summary>
    /// <remarks>
    /// Deliberately not treated as future income anywhere. Salary is money an
    /// employer is contractually going to pay on a known date; an unpaid invoice
    /// is a hope with a name on it. Letting it raise "safe to spend" would be the
    /// one case where this app told someone to spend money that may never come.
    /// </remarks>
    public Money Outstanding =>
        Status is FreelanceJobStatus.Paid or FreelanceJobStatus.Cancelled
            ? Money.Zero(AgreedAmount.Currency)
            : AgreedAmount;

    private FreelanceJob()
    {
        Title = null!;
    }

    public FreelanceJob(
        Guid userId,
        string title,
        Money agreedAmount,
        string? client = null,
        DateOnly? dueOn = null,
        string? notes = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Freelance work must belong to a user.");

        UserId = userId;
        Title = ValidateTitle(title);
        AgreedAmount = ValidateAmount(agreedAmount);
        AmountPaid = Money.Zero(agreedAmount.Currency);
        Client = Trimmed(client);
        DueOn = dueOn;
        Notes = Trimmed(notes);
    }

    /// <summary>Editable only while the money is still in play.</summary>
    public void Update(string title, Money agreedAmount, string? client, DateOnly? dueOn, string? notes)
    {
        if (Status == FreelanceJobStatus.Paid)
            throw new DomainException("Paid work cannot be edited. Delete it and record it again.");

        if (Status == FreelanceJobStatus.Cancelled)
            throw new DomainException("Cancelled work cannot be edited.");

        if (agreedAmount.Currency != AgreedAmount.Currency)
            throw new DomainException("Cannot change the currency of existing freelance work.");

        Title = ValidateTitle(title);
        AgreedAmount = ValidateAmount(agreedAmount);
        Client = Trimmed(client);
        DueOn = dueOn;
        Notes = Trimmed(notes);
        Touch();
    }

    /// <summary>Records that the work was finished and handed over.</summary>
    public void MarkDelivered(DateOnly deliveredOn)
    {
        if (Status == FreelanceJobStatus.Paid)
            throw new DomainException("This work has already been paid.");

        if (Status == FreelanceJobStatus.Cancelled)
            throw new DomainException("Cancelled work cannot be delivered.");

        DeliveredOn = deliveredOn;
        Touch();
    }

    /// <summary>
    /// Records the payment. The caller is responsible for depositing the money
    /// and writing the movement — this entity only knows that it happened.
    /// </summary>
    /// <remarks>
    /// Delivery is implied rather than required. Plenty of freelance work is paid
    /// up front, and refusing to record that would push the user into entering a
    /// delivery date they made up.
    /// </remarks>
    public void MarkPaid(Money amountPaid, Guid depositAccountId, DateOnly paidOn)
    {
        if (Status == FreelanceJobStatus.Paid)
            throw new DomainException("This work has already been paid.");

        if (Status == FreelanceJobStatus.Cancelled)
            throw new DomainException("Cancelled work cannot be paid.");

        if (amountPaid.IsZero || amountPaid.IsNegative)
            throw new DomainException("Payment amount must be greater than zero.");

        if (amountPaid.Currency != AgreedAmount.Currency)
            throw new DomainException("Payment must be in the same currency as the agreed amount.");

        if (depositAccountId == Guid.Empty)
            throw new DomainException("A deposit account is required.");

        AmountPaid = amountPaid;
        DepositAccountId = depositAccountId;
        PaidOn = paidOn;
        DeliveredOn ??= paidOn;
        Touch();
    }

    /// <summary>
    /// Calls the work off. Kept as a row rather than deleted, so a client who
    /// wasted three weeks of your time is still visible next year.
    /// </summary>
    public void Cancel(DateOnly cancelledOn)
    {
        if (Status == FreelanceJobStatus.Paid)
            throw new DomainException("Paid work cannot be cancelled.");

        if (Status == FreelanceJobStatus.Cancelled)
            throw new DomainException("This work is already cancelled.");

        CancelledOn = cancelledOn;
        Touch();
    }

    /// <summary>Undoes a cancellation, for the case where the client comes back.</summary>
    public void Reopen()
    {
        if (Status != FreelanceJobStatus.Cancelled)
            throw new DomainException("Only cancelled work can be reopened.");

        CancelledOn = null;
        Touch();
    }

    private static string ValidateTitle(string title) =>
        !string.IsNullOrWhiteSpace(title)
            ? title.Trim()
            : throw new DomainException("A description of the work is required.");

    private static Money ValidateAmount(Money amount) =>
        amount.IsZero || amount.IsNegative
            ? throw new DomainException("Agreed amount must be greater than zero.")
            : amount;

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
