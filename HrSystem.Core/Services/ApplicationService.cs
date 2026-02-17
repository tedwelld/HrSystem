using HrSystem.Core.Dtos.Applications;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class ApplicationService(
    HrSystemDbContext dbContext,
    ICvService cvService,
    INotificationService notificationService,
    ISnapshotService snapshotService) : IApplicationService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly ICvService _cvService = cvService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ISnapshotService _snapshotService = snapshotService;

    public async Task<JobApplicationDto> ApplyAsync(int candidateId, ApplyForJobDto dto)
    {
        var candidate = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == candidateId && x.IsActive)
            ?? throw new InvalidOperationException("Candidate not found.");

        if (candidate.Role != UserRole.Candidate)
        {
            throw new UnauthorizedAccessException("Only candidate users can apply for jobs.");
        }

        var job = await _dbContext.JobPostings
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == dto.JobPostingId && x.IsOpen)
            ?? throw new InvalidOperationException("Job posting not found or closed.");

        var exists = await _dbContext.JobApplications.AnyAsync(x => x.CandidateId == candidateId && x.JobPostingId == dto.JobPostingId);
        if (exists)
        {
            throw new InvalidOperationException("You have already applied for this job.");
        }

        CvProfile? cv = null;
        if (dto.CvProfileId.HasValue)
        {
            cv = await _dbContext.CvProfiles.FirstOrDefaultAsync(x => x.Id == dto.CvProfileId.Value && x.CandidateId == candidateId)
                ?? throw new InvalidOperationException("CV profile not found for this candidate.");
        }
        else
        {
            cv = await _dbContext.CvProfiles
                .Where(x => x.CandidateId == candidateId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }

        string strengths;
        string weaknesses;
        decimal score;

        if (cv is null)
        {
            strengths = "Application submitted without an uploaded CV profile.";
            weaknesses = "Upload a structured CV to unlock stronger matching analysis.";
            score = 20m;
        }
        else
        {
            var analysis = await _cvService.AnalyzeCvForJobAsync(cv.Id, job.Id);
            strengths = analysis.Strengths;
            weaknesses = analysis.Weaknesses;
            score = analysis.MatchScore;
        }

        var entity = new JobApplication
        {
            JobPostingId = job.Id,
            CandidateId = candidateId,
            CvProfileId = cv?.Id,
            CoverLetter = dto.CoverLetter.Trim(),
            StrengthsSummary = strengths,
            WeaknessesSummary = weaknesses,
            MatchScore = score,
            Stage = ApplicationStage.Applied,
            SubmittedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Scorecard = BuildScorecard(score, strengths, weaknesses, cv is not null)
        };

        _dbContext.JobApplications.Add(entity);
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: candidateId,
            source: "Application",
            action: "Apply",
            category: "JobApplication",
            relatedEntityId: entity.Id,
            details: $"{candidate.Email} applied for '{job.Title}'.",
            notifyAdmins: true);

        await _notificationService.CreateNotificationAsync(
            userId: candidateId,
            title: "Application submitted",
            message: $"Your application for '{job.Title}' was submitted successfully.",
            type: NotificationType.ApplicationSubmitted,
            relatedJobId: job.Id,
            sendEmail: true,
            sendSms: true);

        var adminIds = await _dbContext.Users
            .Where(x => x.Role == UserRole.Admin && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var adminId in adminIds)
        {
            await _notificationService.CreateNotificationAsync(
                userId: adminId,
                title: "New applicant",
                message: $"{candidate.FirstName} {candidate.LastName} applied for '{job.Title}'.",
                type: NotificationType.ApplicationSubmitted,
                relatedJobId: job.Id,
                sendEmail: true,
                sendSms: false);
        }

        return await BuildApplicationDtoAsync(entity.Id)
            ?? throw new InvalidOperationException("Application created but could not be loaded.");
    }

    private static ApplicationScorecard BuildScorecard(decimal score, string strengths, string weaknesses, bool hasCv)
    {
        var skillMatch = Math.Clamp(score, 0m, 100m);
        var experienceScore = hasCv ? Math.Clamp(score - 5m, 0m, 100m) : 20m;
        var educationScore = hasCv ? Math.Clamp(score - 10m, 0m, 100m) : 15m;
        var certificationScore = hasCv ? Math.Clamp(score - 8m, 0m, 100m) : 10m;

        return new ApplicationScorecard
        {
            SkillMatchScore = skillMatch,
            ExperienceScore = experienceScore,
            EducationScore = educationScore,
            CertificationsScore = certificationScore,
            OverallScore = score,
            StrengthsBreakdown = strengths,
            GapsBreakdown = weaknesses,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    public async Task<List<JobApplicationDto>> GetMyApplicationsAsync(int candidateId)
    {
        var appIds = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.CandidateId == candidateId)
            .OrderByDescending(x => x.SubmittedAtUtc)
            .Select(x => x.Id)
            .ToListAsync();

        var result = new List<JobApplicationDto>();
        foreach (var appId in appIds)
        {
            var dto = await BuildApplicationDtoAsync(appId);
            if (dto is not null) result.Add(dto);
        }

        return result;
    }

    public async Task<List<JobApplicationDto>> GetAllApplicationsAsync()
    {
        var appIds = await _dbContext.JobApplications
            .AsNoTracking()
            .OrderByDescending(x => x.SubmittedAtUtc)
            .Select(x => x.Id)
            .ToListAsync();

        var result = new List<JobApplicationDto>();
        foreach (var appId in appIds)
        {
            var dto = await BuildApplicationDtoAsync(appId);
            if (dto is not null) result.Add(dto);
        }

        return result;
    }

    public async Task<List<JobApplicationDto>> GetApplicationsForJobAsync(int jobId)
    {
        var appIds = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.JobPostingId == jobId)
            .OrderByDescending(x => x.SubmittedAtUtc)
            .Select(x => x.Id)
            .ToListAsync();

        var result = new List<JobApplicationDto>();
        foreach (var appId in appIds)
        {
            var dto = await BuildApplicationDtoAsync(appId);
            if (dto is not null) result.Add(dto);
        }

        return result;
    }

    public async Task<bool> UpdateStageAsync(int adminId, UpdateApplicationStageDto dto)
    {
        var admin = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == adminId && x.IsActive);
        if (admin is null || admin.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only admin users can update application stages.");
        }

        if (!Enum.TryParse<ApplicationStage>(dto.Stage, true, out var stage))
        {
            throw new InvalidOperationException("Invalid application stage.");
        }

        var application = await _dbContext.JobApplications
            .Include(x => x.Candidate)
            .Include(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.Id == dto.ApplicationId);

        if (application is null)
        {
            return false;
        }

        application.Stage = stage;
        application.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: adminId,
            source: "Application",
            action: "UpdateStage",
            category: "JobApplication",
            relatedEntityId: application.Id,
            details: $"Stage changed to {stage} for application #{application.Id}.",
            notifyAdmins: true);

        await _notificationService.CreateNotificationAsync(
            userId: application.CandidateId,
            title: "Application status updated",
            message: $"Your application for '{application.JobPosting?.Title}' moved to '{application.Stage}'.",
            type: NotificationType.ApplicationStatusUpdated,
            relatedJobId: application.JobPostingId,
            sendEmail: true,
            sendSms: true);

        return true;
    }

    public async Task<FollowUpNoteDto> AddFollowUpNoteAsync(int adminId, CreateFollowUpNoteDto dto)
    {
        var admin = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == adminId && x.IsActive)
            ?? throw new InvalidOperationException("Admin user not found.");

        if (admin.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only admin users can add follow-up notes.");
        }

        var application = await _dbContext.JobApplications
            .Include(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.Id == dto.ApplicationId)
            ?? throw new InvalidOperationException("Application not found.");

        var note = new FollowUpNote
        {
            JobApplicationId = application.Id,
            AdminId = adminId,
            Note = dto.Note.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.FollowUpNotes.Add(note);
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: adminId,
            source: "Application",
            action: "FollowUp",
            category: "JobApplication",
            relatedEntityId: application.Id,
            details: $"Follow-up note added on application #{application.Id}.",
            notifyAdmins: true);

        await _notificationService.CreateNotificationAsync(
            userId: application.CandidateId,
            title: "Application follow-up",
            message: $"A follow-up note was added for your application to '{application.JobPosting?.Title}'.",
            type: NotificationType.FollowUpNoteAdded,
            relatedJobId: application.JobPostingId,
            sendEmail: true,
            sendSms: false);

        return new FollowUpNoteDto
        {
            Id = note.Id,
            AdminId = admin.Id,
            AdminName = $"{admin.FirstName} {admin.LastName}",
            Note = note.Note,
            CreatedAtUtc = note.CreatedAtUtc
        };
    }

    private async Task<JobApplicationDto?> BuildApplicationDtoAsync(int id)
    {
        var entity = await _dbContext.JobApplications
            .AsNoTracking()
            .Include(x => x.JobPosting)
            .Include(x => x.Candidate)
            .Include(x => x.Scorecard)
            .Include(x => x.FollowUpNotes)
            .ThenInclude(n => n.Admin)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return null;
        }

        return new JobApplicationDto
        {
            Id = entity.Id,
            JobPostingId = entity.JobPostingId,
            JobTitle = entity.JobPosting?.Title ?? string.Empty,
            CandidateId = entity.CandidateId,
            CandidateName = entity.Candidate is null ? string.Empty : $"{entity.Candidate.FirstName} {entity.Candidate.LastName}",
            CandidateEmail = entity.Candidate?.Email ?? string.Empty,
            CvProfileId = entity.CvProfileId,
            Stage = entity.Stage.ToString(),
            CoverLetter = entity.CoverLetter,
            StrengthsSummary = entity.StrengthsSummary,
            WeaknessesSummary = entity.WeaknessesSummary,
            MatchScore = entity.MatchScore,
            SkillMatchScore = entity.Scorecard?.SkillMatchScore ?? entity.MatchScore,
            ExperienceScore = entity.Scorecard?.ExperienceScore ?? entity.MatchScore,
            EducationScore = entity.Scorecard?.EducationScore ?? entity.MatchScore,
            CertificationsScore = entity.Scorecard?.CertificationsScore ?? entity.MatchScore,
            OverallScore = entity.Scorecard?.OverallScore ?? entity.MatchScore,
            SubmittedAtUtc = entity.SubmittedAtUtc,
            FollowUpNotes = entity.FollowUpNotes
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new FollowUpNoteDto
                {
                    Id = x.Id,
                    AdminId = x.AdminId,
                    AdminName = x.Admin == null ? string.Empty : $"{x.Admin.FirstName} {x.Admin.LastName}",
                    Note = x.Note,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToList()
        };
    }
}
