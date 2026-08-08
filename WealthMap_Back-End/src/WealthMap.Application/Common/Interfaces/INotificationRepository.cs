using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<Notification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Notification>> GetForUserAsync(
        Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default);

    Task<int> CountForUserAsync(Guid userId, bool unreadOnly, CancellationToken ct = default);

    Task<IReadOnlyList<Notification>> GetUnreadForUserAsync(Guid userId, CancellationToken ct = default);
}
