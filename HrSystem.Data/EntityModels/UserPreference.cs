using System.ComponentModel.DataAnnotations;

namespace HrSystem.Data.EntityModels;

public class UserPreference
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(20)]
    public string Theme { get; set; } = "light";

    public bool AutoHideSidebar { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
