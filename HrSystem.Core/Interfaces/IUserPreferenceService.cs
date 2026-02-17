using HrSystem.Core.Dtos.Preferences;

namespace HrSystem.Core.Interfaces;

public interface IUserPreferenceService
{
    Task<UserPreferenceDto> GetAsync(int userId);
    Task<UserPreferenceDto> UpsertAsync(int userId, UpdatePreferenceDto dto);
}
