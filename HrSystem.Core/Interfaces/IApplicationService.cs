using HrSystem.Core.Dtos.Applications;

namespace HrSystem.Core.Interfaces;

public interface IApplicationService
{
    Task<JobApplicationDto> ApplyAsync(int candidateId, ApplyForJobDto dto);
    Task<List<JobApplicationDto>> GetMyApplicationsAsync(int candidateId);
    Task<List<JobApplicationDto>> GetAllApplicationsAsync();
    Task<List<JobApplicationDto>> GetApplicationsForJobAsync(int jobId);
    Task<bool> UpdateStageAsync(int adminId, UpdateApplicationStageDto dto);
    Task<FollowUpNoteDto> AddFollowUpNoteAsync(int adminId, CreateFollowUpNoteDto dto);
}
