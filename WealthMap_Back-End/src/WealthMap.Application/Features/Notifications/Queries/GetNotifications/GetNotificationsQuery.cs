using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Notifications.DTOs;

namespace WealthMap.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(
    Guid UserId,
    bool UnreadOnly = false,
    int Page = 1,
    int PageSize = PagedQueryRules.DefaultPageSize)
    : IQuery<PagedResult<NotificationDto>>, IPagedQuery;
