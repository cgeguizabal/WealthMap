using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Notifications.DTOs;

namespace WealthMap.Application.Features.Notifications.Commands.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid Id, Guid UserId) : ICommand<NotificationDto>;
