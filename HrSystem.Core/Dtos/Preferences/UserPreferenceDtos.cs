using System.ComponentModel.DataAnnotations;

namespace HrSystem.Core.Dtos.Preferences;

public class UserPreferenceDto
{
    public int UserId { get; set; }
    public string Theme { get; set; } = "light";
    public bool AutoHideSidebar { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
}

public class UpdatePreferenceDto
{
    [Required]
    public string Theme { get; set; } = "light";

    public bool AutoHideSidebar { get; set; } = true;
}
