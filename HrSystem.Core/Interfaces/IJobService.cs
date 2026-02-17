using HrSystem.Core.Dtos.Jobs;

namespace HrSystem.Core.Interfaces;

public interface IJobService
{
    Task<List<JobPostingDto>> GetOpenJobsAsync();
    Task<List<JobPostingDto>> GetAllJobsAsync();
    Task<JobPostingDto?> GetJobByIdAsync(int id);
    Task<JobPostingDto> CreateJobAsync(int adminId, CreateJobPostingDto dto);
    Task<bool> UpdateJobAsync(int id, UpdateJobPostingDto dto);
    Task<bool> CloseJobAsync(int id);
    Task<bool> DeleteJobAsync(int id);
}
