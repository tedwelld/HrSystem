using HrSystem.Core.Dtos.Notifications;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class NotificationService(
    HrSystemDbContext dbContext,
    IEmailSender emailSender,
    ISmsSender smsSender) : INotificationService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ISmsSender _smsSender = smsSender;

    public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type.ToString(),
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();
    }

    public Task<int> GetUnreadCountAsync(int userId)
        => _dbContext.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead);

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var items = await _dbContext.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync();

        if (items.Count == 0)
        {
            return 0;
        }

        foreach (var item in items)
        {
            item.IsRead = true;
        }

        await _dbContext.SaveChangesAsync();
        return items.Count;
    }

    public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? relatedJobId = null, bool sendEmail = true, bool sendSms = false)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null)
        {
            return;
        }

        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedJobPostingId = relatedJobId,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        var deliveries = new List<NotificationDelivery>
        {
            new()
            {
                NotificationId = notification.Id,
                Channel = DeliveryChannel.InApp,
                Destination = user.Email,
                IsSuccess = true,
                ProviderResponse = "Stored in-app",
                SentAtUtc = DateTime.UtcNow
            }
        };

        if (sendEmail && !string.IsNullOrWhiteSpace(user.Email))
        {
            var (success, response) = await _emailSender.SendAsync(user.Email, title, message);
            deliveries.Add(new NotificationDelivery
            {
                NotificationId = notification.Id,
                Channel = DeliveryChannel.Email,
                Destination = user.Email,
                IsSuccess = success,
                ProviderResponse = response,
                SentAtUtc = DateTime.UtcNow
            });
        }

        if (sendSms && !string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            var (success, response) = await _smsSender.SendAsync(user.PhoneNumber, message);
            deliveries.Add(new NotificationDelivery
            {
                NotificationId = notification.Id,
                Channel = DeliveryChannel.Sms,
                Destination = user.PhoneNumber,
                IsSuccess = success,
                ProviderResponse = response,
                SentAtUtc = DateTime.UtcNow
            });
        }

        _dbContext.NotificationDeliveries.AddRange(deliveries);
        await _dbContext.SaveChangesAsync();
    }

    public async Task BroadcastToCandidatesAsync(string title, string message, NotificationType type, int? relatedJobId = null, bool sendEmail = true, bool sendSms = true)
    {
        var candidateIds = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Role == UserRole.Candidate && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var candidateId in candidateIds)
        {
            await CreateNotificationAsync(candidateId, title, message, type, relatedJobId, sendEmail, sendSms);
        }
    }
}
