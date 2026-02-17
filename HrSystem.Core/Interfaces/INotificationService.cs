using HrSystem.Core.Dtos.Notifications;
using HrSystem.Data.EntityModels.Enums;

namespace HrSystem.Core.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task<int> MarkAllAsReadAsync(int userId);
    Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? relatedJobId = null, bool sendEmail = true, bool sendSms = false);
    Task BroadcastToCandidatesAsync(string title, string message, NotificationType type, int? relatedJobId = null, bool sendEmail = true, bool sendSms = true);
}
