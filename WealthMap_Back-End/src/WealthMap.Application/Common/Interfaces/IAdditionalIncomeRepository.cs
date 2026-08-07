using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface IAdditionalIncomeRepository : IRepository<AdditionalIncome>
{
    Task<AdditionalIncome?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<AdditionalIncome>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
}