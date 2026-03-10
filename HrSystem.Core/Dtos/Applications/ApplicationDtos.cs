using System.ComponentModel.DataAnnotations;

namespace HrSystem.Core.Dtos.Applications;

public class ApplyForJobDto
{
    [Required]
    public int JobPostingId { get; set; }

    public int? CvProfileId { get; set; }

    [MaxLength(3000)]
    public string CoverLetter { get; set; } = string.Empty;
}

public class UpdateApplicationStageDto
{
    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public string Stage { get; set; } = string.Empty;
}

public class ReviewApplicationDto
{
    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public string Stage { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal? TestScore { get; set; }

    [Required, MaxLength(2000)]
    public string Reply { get; set; } = string.Empty;
}

public class CreateFollowUpNoteDto
{
    [Required]
    public int ApplicationId { get; set; }

    [Required, MaxLength(2000)]
    public string Note { get; set; } = string.Empty;
}

public class FollowUpNoteDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class JobApplicationDto
{
    public int Id { get; set; }
    public int JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public int? CvProfileId { get; set; }
    public string CvOriginalFileName { get; set; } = string.Empty;
    public string CvMimeType { get; set; } = string.Empty;
    public string CvContentText { get; set; } = string.Empty;
    public List<string> CvSkills { get; set; } = [];
    public string CvEducationSummary { get; set; } = string.Empty;
    public int CvYearsOfExperience { get; set; }
    public string CvCertificationsSummary { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string CoverLetter { get; set; } = string.Empty;
    public string StrengthsSummary { get; set; } = string.Empty;
    public string WeaknessesSummary { get; set; } = string.Empty;
    public decimal MatchScore { get; set; }
    public decimal SkillMatchScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal EducationScore { get; set; }
    public decimal CertificationsScore { get; set; }
    public decimal OverallScore { get; set; }
    public decimal? TestScore { get; set; }
    public string AdminReply { get; set; } = string.Empty;
    public int? ReviewedByAdminId { get; set; }
    public string ReviewedByAdminName { get; set; } = string.Empty;
    public DateTime? ReviewedAtUtc { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public List<FollowUpNoteDto> FollowUpNotes { get; set; } = [];
}
