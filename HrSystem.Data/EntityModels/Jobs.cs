using System.ComponentModel.DataAnnotations;

namespace HrSystem.Data.EntityModels;

public class JobPosting
{
    public int Id { get; set; }

    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    public int PostedByAdminId { get; set; }
    public User? PostedByAdmin { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(80)]
    public string EmploymentType { get; set; } = string.Empty;

    [MaxLength(80)]
    public string ExperienceLevel { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string RequiredSkillsCsv { get; set; } = string.Empty;

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    public bool IsOpen { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
