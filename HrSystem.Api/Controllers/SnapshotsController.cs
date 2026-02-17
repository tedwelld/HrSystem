using HrSystem.Api.Extensions;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/snapshots")]
[Authorize]
public class SnapshotsController(ISnapshotService snapshotService) : ControllerBase
{
    private readonly ISnapshotService _snapshotService = snapshotService;

    [HttpGet("admin/latest")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Latest([FromQuery] int count = 100)
        => Ok(await _snapshotService.GetLatestAsync(count));

    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery] int count = 100)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await _snapshotService.GetMineAsync(userId.Value, count));
    }
}
