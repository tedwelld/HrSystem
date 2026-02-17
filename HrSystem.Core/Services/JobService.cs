using HrSystem.Core.Dtos.Jobs;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class JobService(
    HrSystemDbContext dbContext,
    INotificationService notificationService,
    ISnapshotService snapshotService) : IJobService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ISnapshotService _snapshotService = snapshotService;

    public async Task<List<JobPostingDto>> GetOpenJobsAsync()
    {
        var jobs = await _dbContext.JobPostings
            .AsNoTracking()
            .Include(x => x.Company)
            .Where(x => x.IsOpen)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return jobs.Select(MapToDto).ToList();
    }

    public async Task<List<JobPostingDto>> GetAllJobsAsync()
    {
        var jobs = await _dbContext.JobPostings
            .AsNoTracking()
            .Include(x => x.Company)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return jobs.Select(MapToDto).ToList();
    }

    public async Task<JobPostingDto?> GetJobByIdAsync(int id)
    {
        var job = await _dbContext.JobPostings
            .AsNoTracking()
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id);

        return job is null ? null : MapToDto(job);
    }

    public async Task<JobPostingDto> CreateJobAsync(int adminId, CreateJobPostingDto dto)
    {
        var admin = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == adminId && x.Role == UserRole.Admin && x.IsActive)
            ?? throw new UnauthorizedAccessException("Only active admin users can create jobs.");

        var company = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == dto.CompanyId)
            ?? throw new InvalidOperationException("Company not found.");

        var entity = new JobPosting
        {
            CompanyId = company.Id,
            PostedByAdminId = admin.Id,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Location = dto.Location.Trim(),
            EmploymentType = dto.EmploymentType.Trim(),
            ExperienceLevel = dto.ExperienceLevel.Trim(),
            RequiredSkillsCsv = string.Join(',', NormalizeSkills(dto.RequiredSkills)),
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            IsOpen = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.JobPostings.Add(entity);
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: adminId,
            source: "Job",
            action: "Create",
            category: "JobPosting",
            relatedEntityId: entity.Id,
            details: $"Posted job '{entity.Title}' in {entity.Location}.",
            notifyAdmins: true);

        await _notificationService.BroadcastToCandidatesAsync(
            title: $"New job posted: {entity.Title}",
            message: $"A new opening is now available in {entity.Location}. Check and apply from your dashboard.",
            type: NotificationType.JobPosted,
            relatedJobId: entity.Id,
            sendEmail: true,
            sendSms: true);

        entity.Company = company;
        return MapToDto(entity);
    }

    public async Task<bool> UpdateJobAsync(int id, UpdateJobPostingDto dto)
    {
        var job = await _dbContext.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
        if (job is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(dto.Title)) job.Title = dto.Title.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Description)) job.Description = dto.Description.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Location)) job.Location = dto.Location.Trim();
        if (!string.IsNullOrWhiteSpace(dto.EmploymentType)) job.EmploymentType = dto.EmploymentType.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ExperienceLevel)) job.ExperienceLevel = dto.ExperienceLevel.Trim();

        if (dto.RequiredSkills is not null)
        {
            job.RequiredSkillsCsv = string.Join(',', NormalizeSkills(dto.RequiredSkills));
        }

        if (dto.SalaryMin.HasValue) job.SalaryMin = dto.SalaryMin.Value;
        if (dto.SalaryMax.HasValue) job.SalaryMax = dto.SalaryMax.Value;
        if (dto.IsOpen.HasValue) job.IsOpen = dto.IsOpen.Value;

        job.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: job.PostedByAdminId,
            source: "Job",
            action: "Update",
            category: "JobPosting",
            relatedEntityId: job.Id,
            details: $"Updated job '{job.Title}'.",
            notifyAdmins: true);

        return true;
    }

    public async Task<bool> CloseJobAsync(int id)
    {
        var job = await _dbContext.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
        if (job is null)
        {
            return false;
        }

        job.IsOpen = false;
        job.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: job.PostedByAdminId,
            source: "Job",
            action: "Close",
            category: "JobPosting",
            relatedEntityId: job.Id,
            details: $"Closed job '{job.Title}'.",
            notifyAdmins: true);

        return true;
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        var job = await _dbContext.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
        if (job is null)
        {
            return false;
        }

        var snapshotActorId = job.PostedByAdminId;
        var title = job.Title;

        _dbContext.JobPostings.Remove(job);
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: snapshotActorId,
            source: "Job",
            action: "Delete",
            category: "JobPosting",
            relatedEntityId: id,
            details: $"Deleted job '{title}'.",
            notifyAdmins: true);

        return true;
    }

    private static List<string> NormalizeSkills(IEnumerable<string> skills)
        => skills
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

    private static JobPostingDto MapToDto(JobPosting x)
    {
        return new JobPostingDto
        {
            Id = x.Id,
            CompanyId = x.CompanyId,
            CompanyName = x.Company?.Name ?? string.Empty,
            PostedByAdminId = x.PostedByAdminId,
            Title = x.Title,
            Description = x.Description,
            Location = x.Location,
            EmploymentType = x.EmploymentType,
            ExperienceLevel = x.ExperienceLevel,
            RequiredSkills = x.RequiredSkillsCsv == string.Empty
                ? []
                : x.RequiredSkillsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
            SalaryMin = x.SalaryMin,
            SalaryMax = x.SalaryMax,
            IsOpen = x.IsOpen,
            CreatedAtUtc = x.CreatedAtUtc
        };
    }
}
