using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IStoreRepository : IRepository<Store>
{
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default);
}