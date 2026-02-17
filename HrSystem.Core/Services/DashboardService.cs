using HrSystem.Core.Dtos.Dashboard;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class DashboardService(HrSystemDbContext dbContext, INotificationService notificationService) : IDashboardService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var sixMonthsAgo = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);

        var totalCandidates = await _dbContext.Users.CountAsync(x => x.Role == UserRole.Candidate && x.IsActive);
        var totalAdmins = await _dbContext.Users.CountAsync(x => x.Role == UserRole.Admin && x.IsActive);
        var totalCompanies = await _dbContext.Companies.CountAsync();
        var openJobs = await _dbContext.JobPostings.CountAsync(x => x.IsOpen);
        var closedJobs = await _dbContext.JobPostings.CountAsync(x => !x.IsOpen);
        var totalApplications = await _dbContext.JobApplications.CountAsync();
        var pendingReview = await _dbContext.JobApplications.CountAsync(x => x.Stage == ApplicationStage.Applied || x.Stage == ApplicationStage.UnderReview);
        var interviewsScheduled = await _dbContext.InterviewSchedules.CountAsync();
        var avgMatchScore = await _dbContext.JobApplications
            .Select(x => (decimal?)x.MatchScore)
            .AverageAsync() ?? 0m;

        var jobsByMonthRaw = await _dbContext.JobPostings
            .Where(x => x.CreatedAtUtc >= sixMonthsAgo)
            .GroupBy(x => new { x.CreatedAtUtc.Year, x.CreatedAtUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var applicationsByMonthRaw = await _dbContext.JobApplications
            .Where(x => x.SubmittedAtUtc >= sixMonthsAgo)
            .GroupBy(x => new { x.SubmittedAtUtc.Year, x.SubmittedAtUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var statusCountsRaw = await _dbContext.JobApplications
            .GroupBy(x => x.Stage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync();

        var monthLabels = Enumerable.Range(0, 6)
            .Select(offset => sixMonthsAgo.AddMonths(offset))
            .Select(date => date.ToString("MMM yyyy"))
            .ToList();

        var jobsByMonth = monthLabels.Select(label => new ChartPointDto
        {
            Label = label,
            Value = jobsByMonthRaw.FirstOrDefault(x => $"{new DateTime(x.Year, x.Month, 1):MMM yyyy}" == label)?.Count ?? 0
        }).ToList();

        var appsByMonth = monthLabels.Select(label => new ChartPointDto
        {
            Label = label,
            Value = applicationsByMonthRaw.FirstOrDefault(x => $"{new DateTime(x.Year, x.Month, 1):MMM yyyy}" == label)?.Count ?? 0
        }).ToList();

        var byStatus = statusCountsRaw
            .OrderBy(x => x.Stage)
            .Select(x => new ChartPointDto { Label = x.Stage.ToString(), Value = x.Count })
            .ToList();

        return new AdminDashboardDto
        {
            TotalCandidates = totalCandidates,
            TotalAdmins = totalAdmins,
            TotalCompanies = totalCompanies,
            OpenJobPostings = openJobs,
            ClosedJobPostings = closedJobs,
            TotalApplications = totalApplications,
            PendingReviewApplications = pendingReview,
            TotalInterviewsScheduled = interviewsScheduled,
            AverageApplicationMatchScore = Math.Round(avgMatchScore, 2),
            JobPostsByMonth = jobsByMonth,
            ApplicationsByMonth = appsByMonth,
            ApplicationsByStatus = byStatus
        };
    }

    public async Task<CandidateDashboardDto> GetCandidateDashboardAsync(int userId)
    {
        var openJobPostings = await _dbContext.JobPostings.CountAsync(x => x.IsOpen);
        var myApplications = await _dbContext.JobApplications.CountAsync(x => x.CandidateId == userId);
        var interviewScheduled = await _dbContext.JobApplications.CountAsync(x => x.CandidateId == userId && x.Stage == ApplicationStage.InterviewScheduled);

        var byStatusRaw = await _dbContext.JobApplications
            .Where(x => x.CandidateId == userId)
            .GroupBy(x => x.Stage)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var unread = await _notificationService.GetUnreadCountAsync(userId);

        return new CandidateDashboardDto
        {
            OpenJobPostings = openJobPostings,
            MyApplications = myApplications,
            InterviewScheduled = interviewScheduled,
            NotificationsUnread = unread,
            MyApplicationsByStatus = byStatusRaw
                .OrderBy(x => x.Key)
                .Select(x => new ChartPointDto { Label = x.Key.ToString(), Value = x.Count })
                .ToList()
        };
    }
}
