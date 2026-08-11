using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface ISalaryDepositRepository : IRepository<SalaryDeposit>
{
    /// <summary>
    /// The paydays already settled for this job inside the range. The poster asks
    /// for this first and skips whatever comes back, which is what keeps repeated
    /// runs from paying the same day twice.
    /// </summary>
    Task<IReadOnlyList<DateOnly>> GetPostedDatesAsync(
        Guid jobId, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<IReadOnlyList<SalaryDeposit>> GetForJobAsync(
        Guid jobId, Guid userId, CancellationToken ct = default);
}
