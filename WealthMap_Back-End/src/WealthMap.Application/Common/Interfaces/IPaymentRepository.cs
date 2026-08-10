using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Common.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetForTargetAsync(
        Guid userId, PaymentTargetType targetType, Guid targetId, CancellationToken ct = default);

    Task<IReadOnlyList<Payment>> GetPagedForUserAsync(
        Guid userId, DateTime? from, DateTime? toExclusive, PaymentTargetType? targetType,
        int page, int pageSize, CancellationToken ct = default);

    Task<int> CountForUserAsync(
        Guid userId, DateTime? from, DateTime? toExclusive, PaymentTargetType? targetType,
        CancellationToken ct = default);

    /// <summary>Every payment in a half-open period, used by the monthly report.</summary>
    Task<IReadOnlyList<Payment>> GetForUserInPeriodAsync(
        Guid userId, DateTime from, DateTime toExclusive, CancellationToken ct = default);
}