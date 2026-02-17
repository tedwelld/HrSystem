using HrSystem.Core.Dtos.Users;

namespace HrSystem.Core.Dtos.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserProfileDto User { get; set; } = new();
}
