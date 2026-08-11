using System.Text.Json.Serialization;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Alerts.DTOs;

/// <summary>
/// An alert carries both the finished English sentence and the parts it was
/// built from.
/// </summary>
/// <remarks>
/// The parts are what let a client say the same thing in another language: given
/// only the sentence there is nothing to rebuild it from. Title and Message stay
/// because the PDF and any caller that does not know the alert types still needs
/// something readable, and they are the fallback when a type has no translation.
///
/// Values are raw, not pre-formatted: amounts as decimals, dates as ISO. The
/// client formats them in its own locale, the way it formats every other figure.
/// </remarks>
public record AlertDto(
    string Type,
    string Severity,
    string Title,
    string Message,
    Guid? RelatedEntityId,
    IReadOnlyDictionary<string, string> Params)
{
    // Round-trips for persisting an alert as a notification; not part of the API shape.
    [JsonIgnore]
    public AlertType TypeValue => Enum.Parse<AlertType>(Type);

    [JsonIgnore]
    public AlertSeverity SeverityValue => Enum.Parse<AlertSeverity>(Severity);

    public static AlertDto Create(
        AlertType type,
        AlertSeverity severity,
        string title,
        string message,
        Guid? relatedEntityId = null,
        IReadOnlyDictionary<string, string>? parameters = null) =>
        new(
            type.ToString(),
            severity.ToString(),
            title,
            message,
            relatedEntityId,
            parameters ?? new Dictionary<string, string>());
}
