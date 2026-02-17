using HrSystem.Core.Dtos.Preferences;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class UserPreferenceService(HrSystemDbContext dbContext) : IUserPreferenceService
{
    private readonly HrSystemDbContext _dbContext = dbContext;

    public async Task<UserPreferenceDto> GetAsync(int userId)
    {
        var pref = await _dbContext.UserPreferences.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
        if (pref is null)
        {
            return new UserPreferenceDto
            {
                UserId = userId,
                Theme = "light",
                AutoHideSidebar = true,
                UpdatedAtUtc = DateTime.UtcNow
            };
        }

        return new UserPreferenceDto
        {
            UserId = pref.UserId,
            Theme = pref.Theme,
            AutoHideSidebar = pref.AutoHideSidebar,
            UpdatedAtUtc = pref.UpdatedAtUtc
        };
    }

    public async Task<UserPreferenceDto> UpsertAsync(int userId, UpdatePreferenceDto dto)
    {
        var theme = dto.Theme.Trim().ToLowerInvariant();
        if (theme is not ("light" or "dark"))
        {
            theme = "light";
        }

        var pref = await _dbContext.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId);
        if (pref is null)
        {
            pref = new UserPreference
            {
                UserId = userId,
                Theme = theme,
                AutoHideSidebar = dto.AutoHideSidebar,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _dbContext.UserPreferences.Add(pref);
        }
        else
        {
            pref.Theme = theme;
            pref.AutoHideSidebar = dto.AutoHideSidebar;
            pref.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return new UserPreferenceDto
        {
            UserId = pref.UserId,
            Theme = pref.Theme,
            AutoHideSidebar = pref.AutoHideSidebar,
            UpdatedAtUtc = pref.UpdatedAtUtc
        };
    }
}
