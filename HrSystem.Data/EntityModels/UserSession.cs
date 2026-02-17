using System.ComponentModel.DataAnnotations;

namespace HrSystem.Data.EntityModels;

public class UserSession
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(300)]
    public string RefreshTokenHash { get; set; } = string.Empty;

    [MaxLength(120)]
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(600)]
    public string UserAgent { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
