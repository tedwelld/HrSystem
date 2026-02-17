using System.ComponentModel.DataAnnotations;
using HrSystem.Data.EntityModels.Enums;

namespace HrSystem.Data.EntityModels;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public int? RelatedJobPostingId { get; set; }
    public JobPosting? RelatedJobPosting { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
}
