using HrSystem.Api.Extensions;
using HrSystem.Core.Dtos.Admin;
using HrSystem.Core.Dtos.Users;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[ApiController]
[Route("api/admin/management")]
[Authorize(Roles = "Admin")]
public class AdminManagementController(IAdminManagementService adminManagementService) : ControllerBase
{
    private readonly IAdminManagementService _adminManagementService = adminManagementService;

    [HttpGet("users")]
    public async Task<IActionResult> Users()
        => Ok(await _adminManagementService.GetUsersAsync());

    [HttpPost("users/hr-admin")]
    public async Task<IActionResult> CreateHrAdmin([FromBody] CreateAdminUserDto dto)
    {
        try
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue) return Unauthorized();

            return Ok(await _adminManagementService.CreateHrAdminAsync(adminId.Value, dto));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        try
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue) return Unauthorized();

            var updated = await _adminManagementService.UpdateUserAsync(adminId.Value, id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue) return Unauthorized();

            var deleted = await _adminManagementService.DeleteUserAsync(adminId.Value, id);
            return deleted ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies")]
    public async Task<IActionResult> Companies()
        => Ok(await _adminManagementService.GetCompaniesAsync());

    [HttpPost("companies")]
    public async Task<IActionResult> CreateCompany([FromBody] HrSystem.Core.Dtos.Admin.CreateCompanyDto dto)
    {
        try
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue) return Unauthorized();

            return Ok(await _adminManagementService.CreateCompanyAsync(adminId.Value, dto));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{id:int}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] AdminUpdateCompanyDto dto)
    {
        try
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue) return Unauthorized();

            var updated = await _adminManagementService.UpdateCompanyAsync(adminId.Value, id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpDelete("companies/{id:int}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        try
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue) return Unauthorized();

            var deleted = await _adminManagementService.DeleteCompanyAsync(adminId.Value, id);
            return deleted ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] AdminSendUserEmailDto dto)
    {
        try
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue) return Unauthorized();

            return Ok(await _adminManagementService.SendUserEmailAsync(adminId.Value, dto));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
