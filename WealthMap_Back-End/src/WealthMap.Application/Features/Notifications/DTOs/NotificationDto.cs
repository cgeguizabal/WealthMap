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
    DateTime CreatedAt,
    /// <summary>
    /// The parts Title and Message were built from, so a client can say the same
    /// thing in another language. Empty for rows raised before this existed —
    /// those fall back to their stored English.
    /// </summary>
    IReadOnlyDictionary<string, string> Params)
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
        notification.CreatedAt,
        notification.Params);
}
