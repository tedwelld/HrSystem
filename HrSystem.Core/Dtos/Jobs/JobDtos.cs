using System.ComponentModel.DataAnnotations;

namespace HrSystem.Core.Dtos.Jobs;

public class CreateJobPostingDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Location { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string EmploymentType { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string ExperienceLevel { get; set; } = string.Empty;

    public List<string> RequiredSkills { get; set; } = [];

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public int CompanyId { get; set; } = 1;
}

public class UpdateJobPostingDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(120)]
    public string? Location { get; set; }

    [MaxLength(80)]
    public string? EmploymentType { get; set; }

    [MaxLength(80)]
    public string? ExperienceLevel { get; set; }

    public List<string>? RequiredSkills { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool? IsOpen { get; set; }
}

public class JobPostingDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int PostedByAdminId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = [];
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool IsOpen { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
