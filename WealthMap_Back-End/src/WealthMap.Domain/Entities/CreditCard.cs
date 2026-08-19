using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

public class CreditCard : BaseEntity
{
    public Guid UserId { get; private set; }
    public string CardName { get; private set; }
    public string BankName { get; private set; }
    public Money CreditLimit { get; private set; }
    public Money UsedCredit { get; private set; }
    public decimal AnnualInterestRate { get; private set; }
    public int PaymentDueDay { get; private set; }
    public int StatementCutoffDay { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Hidden from the user without destroying anything. Purchases, installment
    /// plans and payments reference this card, so a hard delete would be refused
    /// and would take its history with it if it were not.
    /// </summary>
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    /// <summary>
    /// The last four digits a bank prints when naming this card. Identifying data
    /// only — nothing reads it yet.
    /// </summary>
    public string? LastFour { get; private set; }

    /// <summary>
    /// The day this card was reported lost, stolen, damaged or compromised — and so
    /// the day it stopped being spendable. Null while the card is in service.
    /// </summary>
    /// <remarks>
    /// A date rather than a flag, because the date is the fact and "is this card
    /// blocked" is a reading of it. It duplicates the open CardIncident's own
    /// ReportedOn deliberately: LiquidityProjection decides safe-to-spend from the
    /// cards alone, and would otherwise have to load every card's report history to
    /// learn which cards it may still count.
    ///
    /// Blocking stops the card offering headroom. It deliberately does not stop
    /// <see cref="Charge"/>: the charges that most need recording on a stolen card
    /// are the ones the thief made, and a card that refused them would be unusable
    /// exactly when the statement had to be reconciled.
    /// </remarks>
    public DateOnly? BlockedOn { get; private set; }

    public CardLossReason? BlockReason { get; private set; }

    public bool IsBlocked => BlockedOn is not null;

    /// <summary>Manual until a future ingestion feature can honour anything else.</summary>
    public TrackingMode TrackingMode { get; private set; }

    public Money AvailableCredit => CreditLimit - UsedCredit;

    private CreditCard()
{
    CardName = null!;
    BankName = null!;
}

    public CreditCard(
        Guid userId,
        string cardName,
        string bankName,
        Money creditLimit,
        decimal annualInterestRate,
        int paymentDueDay,
        int statementCutoffDay)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Credit card must belong to a user.");

        if (creditLimit.IsNegative || creditLimit.IsZero)
            throw new DomainException("Credit limit must be greater than zero.");

        UserId = userId;
        CardName = ValidateName(cardName, "Card name");
        BankName = ValidateName(bankName, "Bank name");
        CreditLimit = creditLimit;
        UsedCredit = Money.Zero(creditLimit.Currency);
        AnnualInterestRate = ValidateRate(annualInterestRate);
        PaymentDueDay = ValidateDayOfMonth(paymentDueDay, nameof(PaymentDueDay));
        StatementCutoffDay = ValidateDayOfMonth(statementCutoffDay, nameof(StatementCutoffDay));
        TrackingMode = TrackingMode.Manual;
    }

    /// <summary>
    /// Sets or clears the identifying digits.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="UpdateDetails"/> rather than folded into it: this
    /// is the only mutation that can fail because of the *other* field's value, and
    /// every existing caller of UpdateDetails would otherwise have to start passing
    /// two arguments it does not care about.
    /// </remarks>
    public void SetLastFour(string? lastFour)
    {
        var normalized = InstrumentTracking.NormalizeLastFour(lastFour);

        // Checked against the mode already in force, so clearing the digits on a
        // synced card is refused rather than quietly breaking the invariant.
        InstrumentTracking.EnsureIdentifiable(TrackingMode, normalized);

        LastFour = normalized;
        Touch();
    }

    public void SetTrackingMode(TrackingMode mode)
    {
        InstrumentTracking.EnsureIdentifiable(mode, LastFour);

        TrackingMode = mode;
        Touch();
    }

    /// <summary>
    /// Takes the card out of service after it was lost, stolen, damaged or exposed.
    /// </summary>
    /// <remarks>
    /// The balance stays exactly where it was. Losing the plastic does not forgive
    /// what was spent on it, and the statement still falls due on its usual day —
    /// only the card's remaining credit stops counting as money the user can reach.
    /// </remarks>
    public void ReportLost(CardLossReason reason, DateOnly reportedOn)
    {
        if (!Enum.IsDefined(reason))
            throw new DomainException("A card report must say what happened to the card.");

        if (IsArchived)
            throw new DomainException(
                $"Card '{CardName}' is archived. Restore it before reporting it lost.");

        if (IsBlocked)
            throw new DomainException(
                $"Card '{CardName}' was already reported on {BlockedOn:yyyy-MM-dd}. " +
                "Record its replacement or mark it found first.");

        BlockedOn = reportedOn;
        BlockReason = reason;
        Touch();
    }

    /// <summary>
    /// Puts the card back in service under the number the replacement arrived with.
    /// </summary>
    /// <param name="newLastFour">
    /// The new digits, or null when the bank reissued the same number. Null means
    /// "unchanged" rather than "cleared" — a replacement is never the moment to
    /// forget which card this is.
    /// </param>
    public void CompleteReplacement(string? newLastFour)
    {
        EnsureBlocked("replace");

        var normalized = InstrumentTracking.NormalizeLastFour(newLastFour);

        if (normalized is not null)
        {
            InstrumentTracking.EnsureIdentifiable(TrackingMode, normalized);
            LastFour = normalized;
        }

        BlockedOn = null;
        BlockReason = null;
        Touch();
    }

    /// <summary>Puts the card back in service unchanged, because it turned up.</summary>
    public void MarkRecovered()
    {
        EnsureBlocked("mark as found");

        BlockedOn = null;
        BlockReason = null;
        Touch();
    }

    private void EnsureBlocked(string action)
    {
        if (!IsBlocked)
            throw new DomainException(
                $"Cannot {action} card '{CardName}': it was never reported lost or stolen.");
    }

    public void Charge(Money amount)
    {
        EnsureValidAmount(amount);

        var newUsed = UsedCredit + amount;

        if (newUsed > CreditLimit)
            throw new DomainException(
                $"Charge declined on '{CardName}'. Available credit: {AvailableCredit}, requested: {amount}.");

        UsedCredit = newUsed;
        Touch();
    }

    /// <summary>
    /// Undoes a charge that should never have been made.
    /// </summary>
    /// <remarks>
    /// Distinct from <see cref="RegisterPayment"/> even though both reduce the
    /// balance, because they mean opposite things: a payment is money that left an
    /// account, while this is the correction of a record. Routing a correction
    /// through RegisterPayment would put a payment the user never made into the
    /// arithmetic that decides what is still owed.
    ///
    /// Refused when the balance has already fallen below the charge — paid down
    /// since, most likely. Allowing it would drive UsedCredit negative, which the
    /// card cannot represent, and would quietly invent credit that is not there.
    /// </remarks>
    public void ReverseCharge(Money amount)
    {
        EnsureValidAmount(amount);

        if (amount > UsedCredit)
            throw new DomainException(
                $"Cannot reverse {amount} on '{CardName}': only {UsedCredit} is still owed. " +
                "The balance has been paid down since this charge was made.");

        UsedCredit = UsedCredit - amount;
        Touch();
    }

    public void RegisterPayment(Money amount)
    {
        EnsureValidAmount(amount);

        if (amount > UsedCredit)
            throw new DomainException(
                $"Payment exceeds the balance owed on '{CardName}'. Owed: {UsedCredit}, payment: {amount}.");

        UsedCredit = UsedCredit - amount;
        Touch();
    }

    public void Archive()
    {
        if (IsArchived)
            throw new DomainException($"Card '{CardName}' is already archived.");

        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        Touch();
    }

    public void Restore()
    {
        if (!IsArchived)
            throw new DomainException($"Card '{CardName}' is not archived.");

        IsArchived = false;
        ArchivedAt = null;
        Touch();
    }

    public void UpdateCreditLimit(Money newLimit)
    {
        if (newLimit.Currency != CreditLimit.Currency)
            throw new DomainException("Cannot change the currency of an existing card.");

        if (newLimit < UsedCredit)
            throw new DomainException(
                $"New limit {newLimit} is below the current balance owed ({UsedCredit}).");

        CreditLimit = newLimit;
        Touch();
    }

    public void UpdateDetails(
        string cardName,
        string bankName,
        decimal annualInterestRate,
        int paymentDueDay,
        int statementCutoffDay,
        string? notes)
    {
        CardName = ValidateName(cardName, "Card name");
        BankName = ValidateName(bankName, "Bank name");
        AnnualInterestRate = ValidateRate(annualInterestRate);
        PaymentDueDay = ValidateDayOfMonth(paymentDueDay, nameof(PaymentDueDay));
        StatementCutoffDay = ValidateDayOfMonth(statementCutoffDay, nameof(StatementCutoffDay));
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Touch();
    }

    private void EnsureValidAmount(Money amount)
    {
        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Amount must be greater than zero.");

        if (amount.Currency != CreditLimit.Currency)
            throw new DomainException(
                $"Cannot operate in {amount.Currency} on a card held in {CreditLimit.Currency}.");
    }

    private static string ValidateName(string value, string field) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new DomainException($"{field} is required.");

    private static decimal ValidateRate(decimal rate) =>
        rate is >= 0 and <= 200
            ? rate
            : throw new DomainException("Annual interest rate must be between 0 and 200.");

    private static int ValidateDayOfMonth(int day, string field) =>
        day is >= 1 and <= 31
            ? day
            : throw new DomainException($"{field} must be between 1 and 31.");
}