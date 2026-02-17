namespace HrSystem.Core.Dtos.Dashboard;

public class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class AdminDashboardDto
{
    public int TotalCandidates { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalCompanies { get; set; }
    public int OpenJobPostings { get; set; }
    public int ClosedJobPostings { get; set; }
    public int TotalApplications { get; set; }
    public int PendingReviewApplications { get; set; }
    public int TotalInterviewsScheduled { get; set; }
    public decimal AverageApplicationMatchScore { get; set; }
    public List<ChartPointDto> JobPostsByMonth { get; set; } = [];
    public List<ChartPointDto> ApplicationsByMonth { get; set; } = [];
    public List<ChartPointDto> ApplicationsByStatus { get; set; } = [];
}

public class CandidateDashboardDto
{
    public int OpenJobPostings { get; set; }
    public int MyApplications { get; set; }
    public int InterviewScheduled { get; set; }
    public int NotificationsUnread { get; set; }
    public List<ChartPointDto> MyApplicationsByStatus { get; set; } = [];
}
