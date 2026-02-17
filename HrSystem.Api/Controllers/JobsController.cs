using HrSystem.Api.Extensions;
using HrSystem.Core.Dtos.Jobs;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController(IJobService jobService) : ControllerBase
{
    private readonly IJobService _jobService = jobService;

    [HttpGet("open")]
    [Authorize]
    public async Task<IActionResult> GetOpenJobs()
        => Ok(await _jobService.GetOpenJobsAsync());

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllJobs()
        => Ok(await _jobService.GetAllJobsAsync());

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateJobPostingDto dto)
    {
        var adminId = User.GetUserId();
        if (!adminId.HasValue)
        {
            return Unauthorized();
        }

        var created = await _jobService.CreateJobAsync(adminId.Value, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJobPostingDto dto)
    {
        var success = await _jobService.UpdateJobAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Close(int id)
    {
        var success = await _jobService.CloseJobAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _jobService.DeleteJobAsync(id);
        return success ? NoContent() : NotFound();
    }
}
