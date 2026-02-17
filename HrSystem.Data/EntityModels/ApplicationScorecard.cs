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

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}
