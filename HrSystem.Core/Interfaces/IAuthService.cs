using HrSystem.Core.Dtos.Auth;
using HrSystem.Core.Dtos.Users;

namespace HrSystem.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, string ipAddress, string userAgent);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, string ipAddress, string userAgent);
    Task LogoutAsync(int userId, string sessionToken);
    Task<UserProfileDto?> GetUserProfileAsync(int userId);
    Task<UserProfileDto?> UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto);
}
