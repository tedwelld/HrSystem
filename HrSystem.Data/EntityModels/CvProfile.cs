using System.ComponentModel.DataAnnotations;

namespace HrSystem.Data.EntityModels;

public class CvProfile
{
    public int Id { get; set; }

    public int CandidateId { get; set; }
    public User? Candidate { get; set; }

    [MaxLength(250)]
    public string OriginalFileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string StoredFilePath { get; set; } = string.Empty;

    [MaxLength(200)]
    public string MimeType { get; set; } = string.Empty;

    public string ContentText { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string SkillsCsv { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string EducationSummary { get; set; } = string.Empty;

    public int YearsOfExperience { get; set; }

    [MaxLength(1000)]
    public string CertificationsSummary { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
