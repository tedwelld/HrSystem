using HrSystem.Api.Extensions;
using HrSystem.Core.Dtos.Interviews;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/interviews")]
[Authorize]
public class InterviewsController(IInterviewService interviewService) : ControllerBase
{
    private readonly IInterviewService _interviewService = interviewService;

    [HttpGet("admin/mine")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminMine()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        return Ok(await _interviewService.GetForAdminAsync(userId.Value));
    }

    [HttpGet("candidate/mine")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> CandidateMine()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        return Ok(await _interviewService.GetForCandidateAsync(userId.Value));
    }

    [HttpPost("admin/schedule")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Schedule([FromBody] CreateInterviewDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            return Ok(await _interviewService.ScheduleAsync(userId.Value, dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("admin/update-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateInterviewStatusDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var updated = await _interviewService.UpdateStatusAsync(userId.Value, dto);
            return updated ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
