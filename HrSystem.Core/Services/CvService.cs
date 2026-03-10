using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] KnownSkills =
    [
        "c#", "dotnet", "asp.net", "sql", "entity framework", "angular", "react",
        "javascript", "typescript", "azure", "aws", "docker", "kubernetes", "python",
        "java", "node", "html", "css", "git", "rest api", "microservices",
        "recruitment", "communication", "hris", "onboarding"
    ];

    public async Task<CvProfileDto> UploadStructuredCvAsync(int userId, StructuredCvUploadDto dto)
    {
        var normalized = NormalizeStructuredUpload(dto);
        var content = normalized.FullText.Trim();
        var skills = NormalizeSkills(normalized.Skills.Any() ? normalized.Skills : ExtractSkills(content));

        var storedPath = await WriteCvFileAsync(
            normalized.FileName,
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(normalized, JsonOptions)));

        var entity = new CvProfile
        {
            CandidateId = userId,
            OriginalFileName = normalized.FileName,
            StoredFilePath = storedPath,
            MimeType = "application/json",
            ContentText = content,
            SkillsCsv = string.Join(',', skills),
            EducationSummary = normalized.EducationSummary.Trim(),
            YearsOfExperience = normalized.YearsOfExperience,
            CertificationsSummary = normalized.CertificationsSummary.Trim(),
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
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Uploaded CV content is empty.");
        }

        var skills = NormalizeSkills(ExtractSkills(text));

        var storedPath = await WriteCvFileAsync(fileName, Encoding.UTF8.GetBytes(text));

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

    public async Task<CvProfileDto> UploadFileCvAsync(int userId, string fileName, string contentType, byte[] fileBytes)
    {
        if (fileBytes.Length == 0)
        {
            throw new InvalidOperationException("Uploaded CV file is empty.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".json" => await UploadStructuredJsonCvAsync(userId, fileName, fileBytes),
            ".txt" => await UploadTextCvAsync(userId, fileName, "text/plain", DecodeUtf8(fileBytes)),
            ".docx" => await UploadTextCvAsync(
                userId,
                fileName,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ExtractDocxText(fileBytes)),
            _ => throw new InvalidOperationException("Only .json, .txt, and .docx CV files are supported.")
        };
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
                MimeType = x.MimeType,
                ContentText = x.ContentText,
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

    private async Task<string> WriteCvFileAsync(string fileName, byte[] fileBytes)
    {
        var folder = _storageOptions.CvFolder;
        var absoluteFolder = Path.IsPathRooted(folder)
            ? folder
            : Path.Combine(Directory.GetCurrentDirectory(), folder);

        Directory.CreateDirectory(absoluteFolder);

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".txt";
        }

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteFolder, safeFileName);
        await File.WriteAllBytesAsync(absolutePath, fileBytes);

        return absolutePath;
    }

    private static CvProfileDto MapCv(CvProfile entity)
    {
        return new CvProfileDto
        {
            Id = entity.Id,
            OriginalFileName = entity.OriginalFileName,
            MimeType = entity.MimeType,
            ContentText = entity.ContentText,
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

    private async Task<CvProfileDto> UploadStructuredJsonCvAsync(int userId, string fileName, byte[] fileBytes)
    {
        var json = DecodeUtf8(fileBytes);
        StructuredCvUploadDto? dto;

        try
        {
            dto = JsonSerializer.Deserialize<StructuredCvUploadDto>(json, JsonOptions);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("CV JSON does not match the required template.");
        }

        if (dto is null)
        {
            throw new InvalidOperationException("CV JSON does not match the required template.");
        }

        dto.FileName = string.IsNullOrWhiteSpace(dto.FileName) ? fileName : dto.FileName;
        return await UploadStructuredCvAsync(userId, dto);
    }

    private static StructuredCvUploadDto NormalizeStructuredUpload(StructuredCvUploadDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullText))
        {
            throw new InvalidOperationException("Full CV text is required.");
        }

        if (dto.YearsOfExperience < 0)
        {
            throw new InvalidOperationException("Years of experience cannot be negative.");
        }

        var fileName = string.IsNullOrWhiteSpace(dto.FileName) ? "required-cv-template.json" : dto.FileName.Trim();
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}.json";
        }

        return new StructuredCvUploadDto
        {
            FileName = fileName,
            FullText = dto.FullText.Trim(),
            Skills = dto.Skills,
            EducationSummary = dto.EducationSummary ?? string.Empty,
            YearsOfExperience = dto.YearsOfExperience,
            CertificationsSummary = dto.CertificationsSummary ?? string.Empty
        };
    }

    private static string DecodeUtf8(byte[] fileBytes)
    {
        var text = Encoding.UTF8.GetString(fileBytes).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Uploaded CV content is empty.");
        }

        return text;
    }

    private static string ExtractDocxText(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var document = WordprocessingDocument.Open(stream, false);
        var mainPart = document.MainDocumentPart;
        if (mainPart?.Document?.Body is null)
        {
            throw new InvalidOperationException("The uploaded .docx CV could not be read.");
        }

        var texts = mainPart.Document.Body
            .Descendants<Text>()
            .Select(text => text.Text.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();

        var content = string.Join(' ', texts);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("The uploaded .docx CV could not be read.");
        }

        return content;
    }
}
