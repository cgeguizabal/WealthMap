using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Notifications.DTOs;

namespace WealthMap.Application.Features.Notifications.Commands.SyncAlerts;

public record SyncAlertsCommand(Guid UserId) : ICommand<IReadOnlyList<NotificationDto>>;
