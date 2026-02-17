namespace HrSystem.Core.Dtos.Snapshots;

public class SnapshotDto
{
    public long Id { get; set; }
    public int? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? RelatedEntityId { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
