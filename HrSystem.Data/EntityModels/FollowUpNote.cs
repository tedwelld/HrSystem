using System.ComponentModel.DataAnnotations;

namespace HrSystem.Data.EntityModels;

public class FollowUpNote
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public int AdminId { get; set; }
    public User? Admin { get; set; }

    [MaxLength(2000)]
    public string Note { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
