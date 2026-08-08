using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Notifications.DTOs;

namespace WealthMap.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsHandler : IQueryHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationRepository _notifications;

    public GetNotificationsHandler(INotificationRepository notifications) => _notifications = notifications;

    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var totalCount = await _notifications.CountForUserAsync(request.UserId, request.UnreadOnly, ct);

        var items = await _notifications.GetForUserAsync(
            request.UserId, request.UnreadOnly, request.Page, request.PageSize, ct);

        return new PagedResult<NotificationDto>(
            items.Select(NotificationDto.FromEntity).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }
}
