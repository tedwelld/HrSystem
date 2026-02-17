using HrSystem.Api.Extensions;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    private readonly IDashboardService _dashboardService = dashboardService;

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
        => Ok(await _dashboardService.GetAdminDashboardAsync());

    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> CandidateDashboard()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        return Ok(await _dashboardService.GetCandidateDashboardAsync(userId.Value));
    }
}
