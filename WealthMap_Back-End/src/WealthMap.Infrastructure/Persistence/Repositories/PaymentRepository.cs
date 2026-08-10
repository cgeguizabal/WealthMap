using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(WealthMapDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Payment>> GetForTargetAsync(
        Guid userId, PaymentTargetType targetType, Guid targetId, CancellationToken ct = default) =>
        await Set.Where(p => p.UserId == userId && p.TargetType == targetType && p.TargetId == targetId)
                 .OrderByDescending(p => p.OccurredAt)
                 .AsNoTracking()
                 .ToListAsync(ct);

    public async Task<IReadOnlyList<Payment>> GetPagedForUserAsync(
        Guid userId, DateTime? from, DateTime? toExclusive, PaymentTargetType? targetType,
        int page, int pageSize, CancellationToken ct = default) =>
        await Filter(userId, from, toExclusive, targetType)
            .OrderByDescending(p => p.OccurredAt)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<int> CountForUserAsync(
        Guid userId, DateTime? from, DateTime? toExclusive, PaymentTargetType? targetType,
        CancellationToken ct = default) =>
        await Filter(userId, from, toExclusive, targetType).CountAsync(ct);

    public async Task<IReadOnlyList<Payment>> GetForUserInPeriodAsync(
        Guid userId, DateTime from, DateTime toExclusive, CancellationToken ct = default) =>
        await Set.Where(p => p.UserId == userId && p.OccurredAt >= from && p.OccurredAt < toExclusive)
                 .AsNoTracking()
                 .ToListAsync(ct);

    private IQueryable<Payment> Filter(
        Guid userId, DateTime? from, DateTime? toExclusive, PaymentTargetType? targetType)
    {
        var query = Set.Where(p => p.UserId == userId);

        if (from is not null)
            query = query.Where(p => p.OccurredAt >= from);

        if (toExclusive is not null)
            query = query.Where(p => p.OccurredAt < toExclusive);

        if (targetType is not null)
            query = query.Where(p => p.TargetType == targetType);

        return query;
    }
}