using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Entities;

/// <summary>
/// Which account to assume when a bank's transfer notification does not say.
/// </summary>
/// <remarks>
/// Some banks send "transferencia recibida" without naming the destination, so
/// there is nothing in the message to match against an instrument's last four
/// digits. The user nominates a fallback per bank and per direction — inbound and
/// outbound are separate because a household commonly receives into one account
/// and pays out of another.
///
/// Nothing consumes this yet. It is stored now so that the future ingestion has
/// the answer already on record rather than having to interrupt the user for it.
/// </remarks>
public class BankDefault : BaseEntity
{
    public Guid UserId { get; private set; }
    public string BankName { get; private set; }
    public TransferDirection Direction { get; private set; }
    public Guid DefaultAccountId { get; private set; }

    private BankDefault()
    {
        BankName = null!;
    } // required by EF Core

    public BankDefault(Guid userId, string bankName, TransferDirection direction, Guid defaultAccountId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Bank default must belong to a user.");

        if (defaultAccountId == Guid.Empty)
            throw new DomainException("A default account is required.");

        UserId = userId;
        BankName = ValidateBankName(bankName);
        Direction = direction;
        DefaultAccountId = defaultAccountId;
    }

    public void UpdateDefaultAccount(Guid accountId)
    {
        if (accountId == Guid.Empty)
            throw new DomainException("A default account is required.");

        DefaultAccountId = accountId;
        Touch();
    }

    private static string ValidateBankName(string bankName) =>
        !string.IsNullOrWhiteSpace(bankName)
            ? bankName.Trim()
            : throw new DomainException("Bank name is required.");
}
