using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IAccountMovementRepository : IRepository<AccountMovement>
{
    Task<IReadOnlyList<AccountMovement>> GetPagedForAccountAsync(
        Guid accountId, Guid userId, int page, int pageSize, CancellationToken ct = default);

    Task<int> CountForAccountAsync(Guid accountId, Guid userId, CancellationToken ct = default);
}