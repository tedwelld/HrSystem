using System.ComponentModel.DataAnnotations;
using HrSystem.Data.EntityModels.Enums;

namespace HrSystem.Data.EntityModels;

public class NotificationDelivery
{
    public int Id { get; set; }

    public int NotificationId { get; set; }
    public Notification? Notification { get; set; }

    public DeliveryChannel Channel { get; set; }

    [MaxLength(250)]
    public string Destination { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    [MaxLength(1000)]
    public string ProviderResponse { get; set; } = string.Empty;

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}
