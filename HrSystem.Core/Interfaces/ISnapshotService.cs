using HrSystem.Core.Dtos.Snapshots;

namespace HrSystem.Core.Interfaces;

public interface ISnapshotService
{
    Task CaptureAsync(int? actorUserId, string source, string action, string category, int? relatedEntityId, string details, bool notifyAdmins = false);
    Task<List<SnapshotDto>> GetLatestAsync(int count = 100);
    Task<List<SnapshotDto>> GetMineAsync(int userId, int count = 100);
}
