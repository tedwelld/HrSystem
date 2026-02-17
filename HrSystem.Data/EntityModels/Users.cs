using System.ComponentModel.DataAnnotations;
using HrSystem.Data.EntityModels.Enums;

namespace HrSystem.Data.EntityModels;

public class User
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(300)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Candidate;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CvProfile> CvProfiles { get; set; } = new List<CvProfile>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<JobPosting> PostedJobs { get; set; } = new List<JobPosting>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<FollowUpNote> FollowUpNotes { get; set; } = new List<FollowUpNote>();
    public ICollection<InterviewSchedule> InterviewsAsCandidate { get; set; } = new List<InterviewSchedule>();
    public ICollection<InterviewSchedule> InterviewsAsAdmin { get; set; } = new List<InterviewSchedule>();
    public ICollection<Snapshot> Snapshots { get; set; } = new List<Snapshot>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public UserPreference? Preference { get; set; }
}
