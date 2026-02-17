using HrSystem.Api.Extensions;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        return Ok(await _notificationService.GetNotificationsAsync(userId.Value));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(userId.Value);
        return Ok(new { count });
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var success = await _notificationService.MarkAsReadAsync(userId.Value, id);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var count = await _notificationService.MarkAllAsReadAsync(userId.Value);
        return Ok(new { updated = count });
    }
}
