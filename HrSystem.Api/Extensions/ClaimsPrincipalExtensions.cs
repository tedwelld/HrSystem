using System.Security.Claims;

namespace HrSystem.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var userId) ? userId : null;
    }
}
