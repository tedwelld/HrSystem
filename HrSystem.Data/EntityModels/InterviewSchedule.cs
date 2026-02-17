using System.ComponentModel.DataAnnotations;
using HrSystem.Data.EntityModels.Enums;

namespace HrSystem.Data.EntityModels;

public class InterviewSchedule
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public int CandidateId { get; set; }
    public User? Candidate { get; set; }

    public int AdminId { get; set; }
    public User? Admin { get; set; }

    [MaxLength(80)]
    public string InterviewType { get; set; } = "Screening";

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public DateTime ScheduledStartUtc { get; set; }
    public DateTime ScheduledEndUtc { get; set; }

    [MaxLength(80)]
    public string TimeZone { get; set; } = "UTC";

    [MaxLength(500)]
    public string MeetingLinkOrLocation { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
