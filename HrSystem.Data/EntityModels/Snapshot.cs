using System.ComponentModel.DataAnnotations;

namespace HrSystem.Data.EntityModels;

public class Snapshot
{
    public long Id { get; set; }

    public int? ActorUserId { get; set; }
    public User? ActorUser { get; set; }

    [MaxLength(120)]
    public string Source { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Category { get; set; } = string.Empty;

    public int? RelatedEntityId { get; set; }

    [MaxLength(2000)]
    public string Details { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
