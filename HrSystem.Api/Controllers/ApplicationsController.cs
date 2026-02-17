using HrSystem.Api.Extensions;
using HrSystem.Core.Dtos.Applications;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController(IApplicationService applicationService) : ControllerBase
{
    private readonly IApplicationService _applicationService = applicationService;

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Apply([FromBody] ApplyForJobDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _applicationService.ApplyAsync(userId.Value, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Mine()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        return Ok(await _applicationService.GetMyApplicationsAsync(userId.Value));
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllForAdmin()
        => Ok(await _applicationService.GetAllApplicationsAsync());

    [HttpGet("admin/job/{jobId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ByJob(int jobId)
        => Ok(await _applicationService.GetApplicationsForJobAsync(jobId));

    [HttpPost("admin/update-stage")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStage([FromBody] UpdateApplicationStageDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var success = await _applicationService.UpdateStageAsync(userId.Value, dto);
            return success ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("admin/follow-up")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddFollowUp([FromBody] CreateFollowUpNoteDto dto)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var note = await _applicationService.AddFollowUpNoteAsync(userId.Value, dto);
        return Ok(note);
    }
}
