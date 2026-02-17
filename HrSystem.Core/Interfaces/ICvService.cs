using HrSystem.Core.Dtos.Cv;

namespace HrSystem.Core.Interfaces;

public interface ICvService
{
    Task<CvProfileDto> UploadStructuredCvAsync(int userId, StructuredCvUploadDto dto);
    Task<CvProfileDto> UploadTextCvAsync(int userId, string fileName, string contentType, string rawText);
    Task<List<CvProfileDto>> GetMyCvProfilesAsync(int userId);
    Task<(string Strengths, string Weaknesses, decimal MatchScore)> AnalyzeCvForJobAsync(int cvProfileId, int jobPostingId);
}
