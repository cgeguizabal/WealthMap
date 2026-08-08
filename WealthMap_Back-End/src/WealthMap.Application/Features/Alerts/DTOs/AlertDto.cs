using System.Text.Json.Serialization;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Alerts.DTOs;

public record AlertDto(
    string Type,
    string Severity,
    string Title,
    string Message,
    Guid? RelatedEntityId)
{
    // Round-trips for persisting an alert as a notification; not part of the API shape.
    [JsonIgnore]
    public AlertType TypeValue => Enum.Parse<AlertType>(Type);

    [JsonIgnore]
    public AlertSeverity SeverityValue => Enum.Parse<AlertSeverity>(Severity);

    public static AlertDto Create(
        AlertType type, AlertSeverity severity, string title, string message, Guid? relatedEntityId = null) =>
        new(type.ToString(), severity.ToString(), title, message, relatedEntityId);
}
