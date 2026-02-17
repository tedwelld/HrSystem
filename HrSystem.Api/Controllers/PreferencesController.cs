using HrSystem.Api.Extensions;
using HrSystem.Core.Dtos.Preferences;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/preferences")]
[Authorize]
public class PreferencesController(IUserPreferenceService preferenceService) : ControllerBase
{
    private readonly IUserPreferenceService _preferenceService = preferenceService;

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await _preferenceService.GetAsync(userId.Value));
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update([FromBody] UpdatePreferenceDto dto)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await _preferenceService.UpsertAsync(userId.Value, dto));
    }
}
