using System.ComponentModel.DataAnnotations;

namespace HrSystem.Core.Dtos.Cv;

public class StructuredCvUploadDto
{
    [Required, MaxLength(250)]
    public string FileName { get; set; } = "cv.json";

    [Required]
    public string FullText { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = [];

    [MaxLength(1000)]
    public string EducationSummary { get; set; } = string.Empty;

    public int YearsOfExperience { get; set; }

    [MaxLength(1000)]
    public string CertificationsSummary { get; set; } = string.Empty;
}

public class CvProfileDto
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string EducationSummary { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string CertificationsSummary { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
