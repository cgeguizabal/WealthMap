using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Notifications.DTOs;

public record NotificationDto(
    Guid Id,
    string Type,
    string Severity,
    string Title,
    string Message,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt)
{
    public static NotificationDto FromEntity(Notification notification) => new(
        notification.Id,
        notification.Type.ToString(),
        notification.Severity.ToString(),
        notification.Title,
        notification.Message,
        notification.RelatedEntityId,
        notification.IsRead,
        notification.ReadAt,
        notification.CreatedAt);
}
