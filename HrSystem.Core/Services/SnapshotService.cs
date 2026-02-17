using HrSystem.Core.Dtos.Snapshots;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class SnapshotService(HrSystemDbContext dbContext, INotificationService notificationService) : ISnapshotService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly INotificationService _notificationService = notificationService;

    public async Task CaptureAsync(int? actorUserId, string source, string action, string category, int? relatedEntityId, string details, bool notifyAdmins = false)
    {
        var snapshot = new Snapshot
        {
            ActorUserId = actorUserId,
            Source = source.Trim(),
            Action = action.Trim(),
            Category = category.Trim(),
            RelatedEntityId = relatedEntityId,
            Details = details.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Snapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync();

        if (!notifyAdmins)
        {
            return;
        }

        var actorName = "System";
        if (actorUserId.HasValue)
        {
            var actor = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == actorUserId.Value);
            if (actor is not null)
            {
                actorName = $"{actor.FirstName} {actor.LastName}";
            }
        }

        var adminIds = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Role == UserRole.Admin && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var adminId in adminIds)
        {
            if (actorUserId.HasValue && adminId == actorUserId.Value)
            {
                continue;
            }

            await _notificationService.CreateNotificationAsync(
                userId: adminId,
                title: $"Snapshot: {source} {action}",
                message: $"{actorName}: {details}",
                type: NotificationType.System,
                relatedJobId: relatedEntityId,
                sendEmail: false,
                sendSms: false);
        }
    }

    public async Task<List<SnapshotDto>> GetLatestAsync(int count = 100)
    {
        var normalizedCount = Math.Clamp(count, 1, 500);
        return await _dbContext.Snapshots
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(normalizedCount)
            .Select(x => new SnapshotDto
            {
                Id = x.Id,
                ActorUserId = x.ActorUserId,
                ActorName = x.ActorUser == null ? "System" : $"{x.ActorUser.FirstName} {x.ActorUser.LastName}",
                Source = x.Source,
                Action = x.Action,
                Category = x.Category,
                RelatedEntityId = x.RelatedEntityId,
                Details = x.Details,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task<List<SnapshotDto>> GetMineAsync(int userId, int count = 100)
    {
        var normalizedCount = Math.Clamp(count, 1, 500);
        return await _dbContext.Snapshots
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Where(x => x.ActorUserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(normalizedCount)
            .Select(x => new SnapshotDto
            {
                Id = x.Id,
                ActorUserId = x.ActorUserId,
                ActorName = x.ActorUser == null ? "System" : $"{x.ActorUser.FirstName} {x.ActorUser.LastName}",
                Source = x.Source,
                Action = x.Action,
                Category = x.Category,
                RelatedEntityId = x.RelatedEntityId,
                Details = x.Details,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();
    }
}
