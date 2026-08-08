using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Notifications.DTOs;

namespace WealthMap.Application.Features.Notifications.Commands.MarkNotificationRead;

public class MarkNotificationReadHandler : ICommandHandler<MarkNotificationReadCommand, NotificationDto>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadHandler(INotificationRepository notifications, IUnitOfWork unitOfWork)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationDto> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await _notifications.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Notification", request.Id);

        notification.MarkRead();

        await _unitOfWork.SaveChangesAsync(ct);

        return NotificationDto.FromEntity(notification);
    }
}
