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

    /// <summary>
    /// The parts the sentence was built from — amounts, names, dates.
    /// </summary>
    /// <remarks>
    /// Stored alongside the finished text so the notification can be said in
    /// another language later. Keeping only Title and Message would freeze each
    /// row in the language it was raised in, because there would be nothing left
    /// to rebuild it from. Rows written before this existed have none, and fall
    /// back to their stored English.
    /// </remarks>
    public IReadOnlyDictionary<string, string> Params { get; private set; }

    public Guid? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification()
    {
        Title = null!;
        Message = null!;
        Params = new Dictionary<string, string>();
    }

    public Notification(
        Guid userId,
        AlertType type,
        AlertSeverity severity,
        string title,
        string message,
        Guid? relatedEntityId,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Notification must belong to a user.");

        UserId = userId;
        Type = type;
        Severity = severity;
        Title = ValidateText(title, "Title");
        Message = ValidateText(message, "Message");
        Params = parameters ?? new Dictionary<string, string>();
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
