using System.Text;
using HrSystem.Core.Dtos.Cv;
using HrSystem.Core.Interfaces;
using HrSystem.Core.Options;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HrSystem.Core.Services;

public class CvService(
    HrSystemDbContext dbContext,
    IOptions<StorageOptions> storageOptions,
    ISnapshotService snapshotService) : ICvService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly StorageOptions _storageOptions = storageOptions.Value;
    private readonly ISnapshotService _snapshotService = snapshotService;

    private static readonly string[] KnownSkills =
    [
        "c#", "dotnet", "asp.net", "sql", "entity framework", "angular", "react",
        "javascript", "typescript", "azure", "aws", "docker", "kubernetes", "python",
        "java", "node", "html", "css", "git", "rest api", "microservices"
    ];

    public async Task<CvProfileDto> UploadStructuredCvAsync(int userId, StructuredCvUploadDto dto)
    {
        var content = dto.FullText.Trim();
        var skills = NormalizeSkills(dto.Skills.Any() ? dto.Skills : ExtractSkills(content));

        var storedPath = await WriteCvFileAsync(dto.FileName, "application/json", content);

        var entity = new CvProfile
        {
            CandidateId = userId,
            OriginalFileName = dto.FileName,
            StoredFilePath = storedPath,
            MimeType = "application/json",
            ContentText = content,
            SkillsCsv = string.Join(',', skills),
            EducationSummary = dto.EducationSummary.Trim(),
            YearsOfExperience = dto.YearsOfExperience,
            CertificationsSummary = dto.CertificationsSummary.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.CvProfiles.Add(entity);
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: userId,
            source: "CV",
            action: "UploadStructured",
            category: "CvProfile",
            relatedEntityId: entity.Id,
            details: $"Uploaded structured CV '{entity.OriginalFileName}'.",
            notifyAdmins: true);

        return MapCv(entity);
    }

    public async Task<CvProfileDto> UploadTextCvAsync(int userId, string fileName, string contentType, string rawText)
    {
        var text = rawText.Trim();
        var skills = NormalizeSkills(ExtractSkills(text));

        var storedPath = await WriteCvFileAsync(fileName, contentType, text);

        var entity = new CvProfile
        {
            CandidateId = userId,
            OriginalFileName = fileName,
            StoredFilePath = storedPath,
            MimeType = contentType,
            ContentText = text,
            SkillsCsv = string.Join(',', skills),
            EducationSummary = "",
            YearsOfExperience = InferYearsOfExperience(text),
            CertificationsSummary = InferCertifications(text),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.CvProfiles.Add(entity);
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: userId,
            source: "CV",
            action: "UploadText",
            category: "CvProfile",
            relatedEntityId: entity.Id,
            details: $"Uploaded text CV '{entity.OriginalFileName}'.",
            notifyAdmins: true);

        return MapCv(entity);
    }

    public async Task<List<CvProfileDto>> GetMyCvProfilesAsync(int userId)
    {
        return await _dbContext.CvProfiles
            .AsNoTracking()
            .Where(x => x.CandidateId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new CvProfileDto
            {
                Id = x.Id,
                OriginalFileName = x.OriginalFileName,
                Skills = x.SkillsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
                EducationSummary = x.EducationSummary,
                YearsOfExperience = x.YearsOfExperience,
                CertificationsSummary = x.CertificationsSummary,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task<(string Strengths, string Weaknesses, decimal MatchScore)> AnalyzeCvForJobAsync(int cvProfileId, int jobPostingId)
    {
        var cv = await _dbContext.CvProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cvProfileId)
            ?? throw new InvalidOperationException("CV profile not found.");

        var job = await _dbContext.JobPostings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == jobPostingId)
            ?? throw new InvalidOperationException("Job posting not found.");

        var cvSkills = NormalizeSkills(cv.SkillsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries));
        if (cvSkills.Count == 0)
        {
            cvSkills = NormalizeSkills(ExtractSkills(cv.ContentText));
        }

        var requiredSkills = NormalizeSkills(job.RequiredSkillsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries));

        if (requiredSkills.Count == 0)
        {
            var strengthsFallback = "General profile completeness looks good, but job has no required skills configured.";
            var weaknessesFallback = "No specific skill gaps detected due to missing required skills in job post.";
            return (strengthsFallback, weaknessesFallback, 80m);
        }

        var matched = requiredSkills.Intersect(cvSkills).ToList();
        var missing = requiredSkills.Except(cvSkills).ToList();

        var baseScore = (decimal)matched.Count / requiredSkills.Count * 100m;
        var experienceBonus = Math.Min(cv.YearsOfExperience * 1.5m, 10m);
        var score = Math.Min(100m, Math.Round(baseScore + experienceBonus, 2));

        var strengths = matched.Count > 0
            ? $"Strong alignment in: {string.Join(", ", matched)}. Experience years: {cv.YearsOfExperience}."
            : "No direct required skill matches were detected from the uploaded CV.";

        var weaknesses = missing.Count > 0
            ? $"Potential gaps for this role: {string.Join(", ", missing)}. Consider highlighting related projects/training."
            : "No major skill gaps detected for this role.";

        return (strengths, weaknesses, score);
    }

    private async Task<string> WriteCvFileAsync(string fileName, string contentType, string content)
    {
        var folder = _storageOptions.CvFolder;
        var absoluteFolder = Path.IsPathRooted(folder)
            ? folder
            : Path.Combine(Directory.GetCurrentDirectory(), folder);

        Directory.CreateDirectory(absoluteFolder);

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType.Contains("json", StringComparison.OrdinalIgnoreCase) ? ".json" : ".txt";
        }

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteFolder, safeFileName);
        await File.WriteAllTextAsync(absolutePath, content, Encoding.UTF8);

        return absolutePath;
    }

    private static CvProfileDto MapCv(CvProfile entity)
    {
        return new CvProfileDto
        {
            Id = entity.Id,
            OriginalFileName = entity.OriginalFileName,
            Skills = entity.SkillsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList(),
            EducationSummary = entity.EducationSummary,
            YearsOfExperience = entity.YearsOfExperience,
            CertificationsSummary = entity.CertificationsSummary,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    private static List<string> ExtractSkills(string text)
    {
        var lower = text.ToLowerInvariant();
        return KnownSkills.Where(lower.Contains).ToList();
    }

    private static List<string> NormalizeSkills(IEnumerable<string> skills)
    {
        return skills
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    private static int InferYearsOfExperience(string text)
    {
        var lower = text.ToLowerInvariant();
        for (var i = 20; i >= 1; i--)
        {
            if (lower.Contains($"{i}+ years") || lower.Contains($"{i} years"))
            {
                return i;
            }
        }

        return 0;
    }

    private static string InferCertifications(string text)
    {
        var matches = new List<string>();
        var lower = text.ToLowerInvariant();

        if (lower.Contains("aws certified")) matches.Add("AWS Certified");
        if (lower.Contains("azure certified")) matches.Add("Azure Certified");
        if (lower.Contains("scrum")) matches.Add("Scrum");
        if (lower.Contains("pmp")) matches.Add("PMP");

        return string.Join(", ", matches);
    }
}
