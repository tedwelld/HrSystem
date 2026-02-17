using HrSystem.Core.Dtos.Auth;
using HrSystem.Core.Dtos.Users;

namespace HrSystem.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<UserProfileDto?> GetUserProfileAsync(int userId);
    Task<UserProfileDto?> UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto);
}
