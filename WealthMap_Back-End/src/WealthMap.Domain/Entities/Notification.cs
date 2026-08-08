using WealthMap.Domain.Common;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Entities;

/// <summary>
/// A persisted alert. Alerts are computed live from current data; persisting one
/// records that the user was told, so it can be marked read (and later emailed).
/// </summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public AlertType Type { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification()
    {
        Title = null!;
        Message = null!;
    }

    public Notification(
        Guid userId,
        AlertType type,
        AlertSeverity severity,
        string title,
        string message,
        Guid? relatedEntityId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Notification must belong to a user.");

        UserId = userId;
        Type = type;
        Severity = severity;
        Title = ValidateText(title, "Title");
        Message = ValidateText(message, "Message");
        RelatedEntityId = relatedEntityId;
        IsRead = false;
    }

    public void MarkRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        Touch();
    }

    private static string ValidateText(string value, string field) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new DomainException($"{field} is required.");
}
