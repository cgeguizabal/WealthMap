using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Common.Interfaces;

public interface IBankDefaultRepository : IRepository<BankDefault>
{
    Task<IReadOnlyList<BankDefault>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);

    Task<BankDefault?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// The existing default for this bank and direction, if any. The upsert reads
    /// through this rather than catching a unique-index violation, so the common
    /// path is a plain update instead of a failed insert.
    /// </summary>
    Task<BankDefault?> GetForBankAsync(
        Guid userId, string bankName, TransferDirection direction, CancellationToken ct = default);
}
