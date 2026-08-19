using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.Services;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string BankName { get; private set; }
    public AccountType Type { get; private set; }
    public Money Balance { get; private set; }
    public bool IsBlockedForSaving { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Removed from the user's view without destroying anything. A hard delete
    /// would cascade away this account's movements — the very history the record
    /// exists to preserve — and would be refused outright by purchases, payments
    /// and jobs that reference it.
    /// </summary>
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    /// <summary>
    /// The last four digits of the account number — what a bank prints when it
    /// names this account. Identifying data only; nothing reads it yet.
    /// </summary>
    public string? LastFour { get; private set; }

    /// <summary>Manual until a future ingestion feature can honour anything else.</summary>
    public TrackingMode TrackingMode { get; private set; }

    /// <summary>Whether a debit card reaches this account, and of what kind.</summary>
    public DebitCardType DebitCardType { get; private set; }

    /// <summary>
    /// The last four digits of the debit card, which is a different number from
    /// the account's own.
    /// </summary>
    /// <remarks>
    /// Kept apart from <see cref="LastFour"/> deliberately. A notification about a
    /// card purchase quotes the card; one about a transfer quotes the account. One
    /// field for both would match the wrong message about half the time.
    /// </remarks>
    public string? DebitCardLastFour { get; private set; }

    /// <summary>
    /// The day the debit card was reported lost, stolen, damaged or compromised.
    /// Null while it is in service.
    /// </summary>
    /// <remarks>
    /// Only the card is out of action, never the account. The balance is still
    /// there and still reachable by transfer, in branch, or by the replacement when
    /// it arrives — so nothing here touches what the account is worth. It exists so
    /// the app can say "this card is not usable" rather than showing a number that
    /// no longer opens anything.
    /// </remarks>
    public DateOnly? DebitCardBlockedOn { get; private set; }

    public CardLossReason? DebitCardBlockReason { get; private set; }

    public bool IsDebitCardBlocked => DebitCardBlockedOn is not null;

    private Account()
{
    Name = null!;
    BankName = null!;
}

    public Account(Guid userId, string name, string bankName, AccountType type, Money openingBalance)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Account must belong to a user.");

        if (openingBalance.IsNegative)
            throw new DomainException("Opening balance cannot be negative.");

        UserId = userId;
        Name = ValidateName(name);
        BankName = ValidateBankName(bankName);
        Type = type;
        Balance = openingBalance;
        IsBlockedForSaving = false;
        TrackingMode = TrackingMode.Manual;
        DebitCardType = DebitCardType.None;
    }

    /// <summary>
    /// Records whether a debit card reaches this account, and its digits.
    /// </summary>
    /// <remarks>
    /// Set together because one governs the other: digits without a card describe
    /// nothing, so choosing <see cref="DebitCardType.None"/> clears them rather
    /// than leaving an orphan number behind for a card that does not exist.
    ///
    /// The digits stay optional for a card that does exist. A user may know they
    /// have one without having it to hand, and refusing the answer they can give
    /// in order to demand one they cannot is a bad trade.
    /// </remarks>
    public void SetDebitCard(DebitCardType type, string? lastFour)
    {
        DebitCardType = type;

        DebitCardLastFour = type == DebitCardType.None
            ? null
            : InstrumentTracking.NormalizeLastFour(lastFour);

        Touch();
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
        // synced account is refused rather than quietly breaking the invariant.
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

    /// <summary>Takes the debit card out of service. The account itself is untouched.</summary>
    public void ReportDebitCardLost(CardLossReason reason, DateOnly reportedOn)
    {
        if (!Enum.IsDefined(reason))
            throw new DomainException("A card report must say what happened to the card.");

        if (DebitCardType == DebitCardType.None)
            throw new DomainException(
                $"Account '{Name}' has no debit card to report.");

        if (IsDebitCardBlocked)
            throw new DomainException(
                $"The debit card on '{Name}' was already reported on " +
                $"{DebitCardBlockedOn:yyyy-MM-dd}. Record its replacement or mark it found first.");

        DebitCardBlockedOn = reportedOn;
        DebitCardBlockReason = reason;
        Touch();
    }

    /// <summary>
    /// Puts the debit card back in service under the number the replacement carries.
    /// </summary>
    /// <param name="newLastFour">
    /// The new digits, or null when the bank reissued the same number. Null leaves
    /// the recorded number alone rather than clearing it.
    /// </param>
    public void CompleteDebitCardReplacement(string? newLastFour)
    {
        EnsureDebitCardBlocked("replace");

        var normalized = InstrumentTracking.NormalizeLastFour(newLastFour);

        if (normalized is not null) DebitCardLastFour = normalized;

        DebitCardBlockedOn = null;
        DebitCardBlockReason = null;
        Touch();
    }

    /// <summary>Puts the debit card back in service unchanged, because it turned up.</summary>
    public void MarkDebitCardRecovered()
    {
        EnsureDebitCardBlocked("mark as found");

        DebitCardBlockedOn = null;
        DebitCardBlockReason = null;
        Touch();
    }

    private void EnsureDebitCardBlocked(string action)
    {
        if (!IsDebitCardBlocked)
            throw new DomainException(
                $"Cannot {action} the debit card on '{Name}': it was never reported lost or stolen.");
    }

    public void Deposit(Money amount)
    {
        EnsurePositive(amount);
        Balance = Balance + amount;
        Touch();
    }

    public void Withdraw(Money amount)
    {
        EnsurePositive(amount);

        if (IsBlockedForSaving)
            throw new DomainException(
                $"Account '{Name}' is blocked for saving. Unblock it before taking money out.");

        if (amount > Balance)
            throw new DomainException(
                $"Insufficient funds in '{Name}'. Available: {Balance}, requested: {amount}.");

        Balance = Balance - amount;
        Touch();
    }

    public void BlockForSaving()
    {
        if (IsBlockedForSaving)
            throw new DomainException($"Account '{Name}' is already blocked.");

        IsBlockedForSaving = true;
        Touch();
    }

    public void UnblockForSaving()
    {
        if (!IsBlockedForSaving)
            throw new DomainException($"Account '{Name}' is not blocked.");

        IsBlockedForSaving = false;
        Touch();
    }

    public void Archive()
    {
        if (IsArchived)
            throw new DomainException($"Account '{Name}' is already archived.");

        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        Touch();
    }

    public void Restore()
    {
        if (!IsArchived)
            throw new DomainException($"Account '{Name}' is not archived.");

        IsArchived = false;
        ArchivedAt = null;
        Touch();
    }

    public void UpdateDetails(string name, string bankName, string? notes)
    {
        Name = ValidateName(name);
        BankName = ValidateBankName(bankName);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Touch();
    }

    private void EnsurePositive(Money amount)
    {
        if (amount.IsZero || amount.IsNegative)
            throw new DomainException("Amount must be greater than zero.");

        if (amount.Currency != Balance.Currency)
            throw new DomainException(
                $"Cannot operate in {amount.Currency} on an account held in {Balance.Currency}.");
    }

    private static string ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Account name is required.");

    private static string ValidateBankName(string bankName) =>
        !string.IsNullOrWhiteSpace(bankName)
            ? bankName.Trim()
            : throw new DomainException("Bank name is required.");
}