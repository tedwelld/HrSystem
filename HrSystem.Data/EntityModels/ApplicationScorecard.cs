using System.ComponentModel.DataAnnotations;

namespace HrSystem.Data.EntityModels;

public class ApplicationScorecard
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public decimal SkillMatchScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal EducationScore { get; set; }
    public decimal CertificationsScore { get; set; }
    public decimal OverallScore { get; set; }

    [MaxLength(3000)]
    public string StrengthsBreakdown { get; set; } = string.Empty;

    [MaxLength(3000)]
    public string GapsBreakdown { get; set; } = string.Empty;

    public decimal? TestScore { get; set; }

    [MaxLength(2000)]
    public string ReviewReply { get; set; } = string.Empty;

    public int? ReviewedByAdminId { get; set; }
    public User? ReviewedByAdmin { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}
