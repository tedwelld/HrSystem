using System.Text;
using HrSystem.Api.Extensions;
using HrSystem.Core.Dtos.Cv;
using HrSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "Scoped to controller file for upload request shape.")]
public sealed class TextCvUploadRequest
{
    public IFormFile? File { get; set; }
}

[ApiController]
[Route("api/cv")]
[Authorize(Roles = "Candidate")]
public class CvController(ICvService cvService) : ControllerBase
{
    private readonly ICvService _cvService = cvService;

    [HttpPost("structured")]
    public async Task<IActionResult> UploadStructured([FromBody] StructuredCvUploadDto dto)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _cvService.UploadStructuredCvAsync(userId.Value, dto);
        return Ok(result);
    }

    [HttpPost("text-upload")]
    [RequestSizeLimit(5_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadText([FromForm] TextCvUploadRequest request)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var file = request.File;
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "File is required." });
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new { message = "Uploaded CV content is empty." });
        }

        var result = await _cvService.UploadTextCvAsync(userId.Value, file.FileName, file.ContentType ?? "text/plain", content);
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        return Ok(await _cvService.GetMyCvProfilesAsync(userId.Value));
    }
}
