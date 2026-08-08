using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Notifications.Commands.MarkNotificationRead;
using WealthMap.Application.Features.Notifications.Commands.SyncAlerts;
using WealthMap.Application.Features.Notifications.Queries.GetNotifications;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetNotificationsQuery(User.GetUserId(), unreadOnly, page, pageSize);
        var result = await _sender.Send(query, ct);
        return Ok(result);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(CancellationToken ct)
    {
        var result = await _sender.Send(new SyncAlertsCommand(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new MarkNotificationReadCommand(id, User.GetUserId()), ct);
        return Ok(result);
    }
}
