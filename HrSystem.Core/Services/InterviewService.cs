using HrSystem.Core.Dtos.Interviews;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class InterviewService(
    HrSystemDbContext dbContext,
    INotificationService notificationService,
    ISnapshotService snapshotService) : IInterviewService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ISnapshotService _snapshotService = snapshotService;

    public async Task<List<InterviewDto>> GetForAdminAsync(int adminId)
    {
        var isAdmin = await _dbContext.Users.AnyAsync(x => x.Id == adminId && x.IsActive && x.Role == UserRole.Admin);
        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only active admin users can view admin interviews.");
        }

        return await QueryInterviews()
            .Where(x => x.AdminId == adminId)
            .OrderByDescending(x => x.ScheduledStartUtc)
            .Select(MapToDtoExpression())
            .ToListAsync();
    }

    public async Task<List<InterviewDto>> GetForCandidateAsync(int candidateId)
    {
        var isCandidate = await _dbContext.Users.AnyAsync(x => x.Id == candidateId && x.IsActive && x.Role == UserRole.Candidate);
        if (!isCandidate)
        {
            throw new UnauthorizedAccessException("Only active candidate users can view candidate interviews.");
        }

        return await QueryInterviews()
            .Where(x => x.CandidateId == candidateId)
            .OrderByDescending(x => x.ScheduledStartUtc)
            .Select(MapToDtoExpression())
            .ToListAsync();
    }

    public async Task<InterviewDto> ScheduleAsync(int adminId, CreateInterviewDto dto)
    {
        var admin = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == adminId && x.IsActive && x.Role == UserRole.Admin)
            ?? throw new UnauthorizedAccessException("Only active admin users can schedule interviews.");

        if (dto.ScheduledEndUtc <= dto.ScheduledStartUtc)
        {
            throw new InvalidOperationException("Interview end time must be after start time.");
        }

        var application = await _dbContext.JobApplications
            .Include(x => x.Candidate)
            .Include(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.Id == dto.ApplicationId)
            ?? throw new InvalidOperationException("Application not found.");

        var interview = new InterviewSchedule
        {
            JobApplicationId = application.Id,
            CandidateId = application.CandidateId,
            AdminId = adminId,
            InterviewType = dto.InterviewType.Trim(),
            Status = InterviewStatus.Scheduled,
            ScheduledStartUtc = dto.ScheduledStartUtc,
            ScheduledEndUtc = dto.ScheduledEndUtc,
            TimeZone = dto.TimeZone.Trim(),
            MeetingLinkOrLocation = dto.MeetingLinkOrLocation.Trim(),
            Notes = dto.Notes.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.InterviewSchedules.Add(interview);
        application.Stage = ApplicationStage.InterviewScheduled;
        application.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var title = $"Interview scheduled: {application.JobPosting?.Title}";
        var message = $"Interview on {interview.ScheduledStartUtc:yyyy-MM-dd HH:mm} UTC ({interview.InterviewType}).";

        await _notificationService.CreateNotificationAsync(
            userId: application.CandidateId,
            title: title,
            message: message,
            type: NotificationType.InterviewScheduled,
            relatedJobId: application.JobPostingId,
            sendEmail: true,
            sendSms: true);

        await _snapshotService.CaptureAsync(
            actorUserId: adminId,
            source: "Interview",
            action: "Schedule",
            category: "InterviewSchedule",
            relatedEntityId: interview.Id,
            details: $"Scheduled interview for application #{application.Id}.",
            notifyAdmins: true);

        return await QueryInterviews()
            .Where(x => x.Id == interview.Id)
            .Select(MapToDtoExpression())
            .FirstAsync();
    }

    public async Task<bool> UpdateStatusAsync(int adminId, UpdateInterviewStatusDto dto)
    {
        var isAdmin = await _dbContext.Users.AnyAsync(x => x.Id == adminId && x.IsActive && x.Role == UserRole.Admin);
        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only active admin users can update interviews.");
        }

        if (!Enum.TryParse<InterviewStatus>(dto.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid interview status.");
        }

        var interview = await _dbContext.InterviewSchedules
            .Include(x => x.JobApplication)
            .FirstOrDefaultAsync(x => x.Id == dto.InterviewId && x.AdminId == adminId);

        if (interview is null)
        {
            return false;
        }

        interview.Status = status;
        interview.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? interview.Notes : dto.Notes.Trim();
        interview.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        if (interview.JobApplication is not null)
        {
            await _notificationService.CreateNotificationAsync(
                userId: interview.JobApplication.CandidateId,
                title: "Interview update",
                message: $"Your interview status was updated to '{status}'.",
                type: NotificationType.InterviewUpdated,
                relatedJobId: interview.JobApplication.JobPostingId,
                sendEmail: true,
                sendSms: false);
        }

        await _snapshotService.CaptureAsync(
            actorUserId: adminId,
            source: "Interview",
            action: "Update",
            category: "InterviewSchedule",
            relatedEntityId: interview.Id,
            details: $"Interview #{interview.Id} updated to {status}.",
            notifyAdmins: true);

        return true;
    }

    private IQueryable<InterviewSchedule> QueryInterviews()
    {
        return _dbContext.InterviewSchedules
            .AsNoTracking()
            .Include(x => x.Candidate)
            .Include(x => x.Admin);
    }

    private static System.Linq.Expressions.Expression<Func<InterviewSchedule, InterviewDto>> MapToDtoExpression()
    {
        return x => new InterviewDto
        {
            Id = x.Id,
            JobApplicationId = x.JobApplicationId,
            CandidateId = x.CandidateId,
            CandidateName = x.Candidate == null ? string.Empty : (x.Candidate.FirstName + " " + x.Candidate.LastName),
            AdminId = x.AdminId,
            AdminName = x.Admin == null ? string.Empty : (x.Admin.FirstName + " " + x.Admin.LastName),
            InterviewType = x.InterviewType,
            Status = x.Status.ToString(),
            ScheduledStartUtc = x.ScheduledStartUtc,
            ScheduledEndUtc = x.ScheduledEndUtc,
            TimeZone = x.TimeZone,
            MeetingLinkOrLocation = x.MeetingLinkOrLocation,
            Notes = x.Notes
        };
    }
}
