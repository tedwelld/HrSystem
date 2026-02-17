using System.ComponentModel.DataAnnotations;
using HrSystem.Data.EntityModels.Enums;

namespace HrSystem.Data.EntityModels;

public class JobApplication
{
    public int Id { get; set; }

    public int JobPostingId { get; set; }
    public JobPosting? JobPosting { get; set; }

    public int CandidateId { get; set; }
    public User? Candidate { get; set; }

    public int? CvProfileId { get; set; }
    public CvProfile? CvProfile { get; set; }

    [MaxLength(3000)]
    public string CoverLetter { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string StrengthsSummary { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string WeaknessesSummary { get; set; } = string.Empty;

    public decimal MatchScore { get; set; }

    public ApplicationStage Stage { get; set; } = ApplicationStage.Applied;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<FollowUpNote> FollowUpNotes { get; set; } = new List<FollowUpNote>();
    public ICollection<InterviewSchedule> InterviewSchedules { get; set; } = new List<InterviewSchedule>();
    public ApplicationScorecard? Scorecard { get; set; }
}
