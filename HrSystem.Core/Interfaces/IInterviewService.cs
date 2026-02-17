using HrSystem.Core.Dtos.Interviews;

namespace HrSystem.Core.Interfaces;

public interface IInterviewService
{
    Task<List<InterviewDto>> GetForAdminAsync(int adminId);
    Task<List<InterviewDto>> GetForCandidateAsync(int candidateId);
    Task<InterviewDto> ScheduleAsync(int adminId, CreateInterviewDto dto);
    Task<bool> UpdateStatusAsync(int adminId, UpdateInterviewStatusDto dto);
}
