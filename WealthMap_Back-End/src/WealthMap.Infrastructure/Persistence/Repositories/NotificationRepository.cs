using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(WealthMapDbContext context) : base(context) { }

    public async Task<Notification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(
        Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default) =>
        await Filter(userId, unreadOnly)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<int> CountForUserAsync(Guid userId, bool unreadOnly, CancellationToken ct = default) =>
        await Filter(userId, unreadOnly).CountAsync(ct);

    public async Task<IReadOnlyList<Notification>> GetUnreadForUserAsync(
        Guid userId, CancellationToken ct = default) =>
        await Filter(userId, unreadOnly: true)
            .AsNoTracking()
            .ToListAsync(ct);

    private IQueryable<Notification> Filter(Guid userId, bool unreadOnly)
    {
        var query = Set.Where(n => n.UserId == userId);
        return unreadOnly ? query.Where(n => !n.IsRead) : query;
    }
}
