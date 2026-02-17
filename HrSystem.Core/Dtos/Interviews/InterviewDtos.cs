namespace HrSystem.Core.Dtos.Interviews;

public class InterviewDto
{
    public int Id { get; set; }
    public int JobApplicationId { get; set; }
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int AdminId { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public string InterviewType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStartUtc { get; set; }
    public DateTime ScheduledEndUtc { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string MeetingLinkOrLocation { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class CreateInterviewDto
{
    public int ApplicationId { get; set; }
    public string InterviewType { get; set; } = "Screening";
    public DateTime ScheduledStartUtc { get; set; }
    public DateTime ScheduledEndUtc { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string MeetingLinkOrLocation { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class UpdateInterviewStatusDto
{
    public int InterviewId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
