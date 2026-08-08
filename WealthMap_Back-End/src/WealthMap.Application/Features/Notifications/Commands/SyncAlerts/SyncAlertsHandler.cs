using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Alerts;
using WealthMap.Application.Features.Notifications.DTOs;
using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Notifications.Commands.SyncAlerts;

/// <summary>
/// Persists the alerts that are currently true and not already sitting unread,
/// so re-running it does not pile up duplicates. Returns only what was created.
/// </summary>
public class SyncAlertsHandler : ICommandHandler<SyncAlertsCommand, IReadOnlyList<NotificationDto>>
{
    private readonly FinancialSnapshotLoader _loader;
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public SyncAlertsHandler(
        FinancialSnapshotLoader loader,
        INotificationRepository notifications,
        IUnitOfWork unitOfWork)
    {
        _loader = loader;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(SyncAlertsCommand request, CancellationToken ct)
    {
        var snapshot = await _loader.LoadAsync(request.UserId, ct);
        var alerts = AlertRules.Evaluate(snapshot);

        var existing = await _notifications.GetUnreadForUserAsync(request.UserId, ct);

        var alreadyRaised = existing
            .Select(n => (n.Type, n.RelatedEntityId))
            .ToHashSet();

        var created = new List<Notification>();

        foreach (var alert in alerts)
        {
            if (!alreadyRaised.Add((alert.TypeValue, alert.RelatedEntityId)))
                continue;

            var notification = new Notification(
                request.UserId,
                alert.TypeValue,
                alert.SeverityValue,
                alert.Title,
                alert.Message,
                alert.RelatedEntityId);

            await _notifications.AddAsync(notification, ct);
            created.Add(notification);
        }

        if (created.Count > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        return created.Select(NotificationDto.FromEntity).ToList();
    }
}
