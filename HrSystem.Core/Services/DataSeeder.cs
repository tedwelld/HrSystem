using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class DataSeeder(HrSystemDbContext dbContext) : IDataSeeder
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private const string SeedAdminEmail = "admin@hrsystem.com";
    private const string SeedAdminPassword = "Admin@HrSystem2026!";

    public async Task SeedAsync()
    {
        await _dbContext.Database.MigrateAsync();

        var admin = await EnsureSeedUserAsync(
            firstName: "System",
            lastName: "Administrator",
            email: SeedAdminEmail,
            phoneNumber: "+12065550999",
            password: SeedAdminPassword,
            role: UserRole.Admin);

        await ReplaceLegacySeedAdminAsync(admin.Id, admin.Email);

        await EnsureSeedUserAsync(
            firstName: "John",
            lastName: "Candidate",
            email: "john.candidate@hrsytem.com",
            phoneNumber: "+12065550111",
            password: "User@12345",
            role: UserRole.Candidate);

        await EnsureSeedUserAsync(
            firstName: "Mary",
            lastName: "Candidate",
            email: "mary.candidate@hrsytem.com",
            phoneNumber: "+12065550112",
            password: "User@12345",
            role: UserRole.Candidate);

        await EnsureDefaultPreferencesAsync();

        var sampleJobExists = await _dbContext.JobPostings.AnyAsync(x => x.Title == "Junior .NET Developer");
        if (!sampleJobExists)
        {
            _dbContext.JobPostings.Add(new JobPosting
            {
                CompanyId = 1,
                PostedByAdminId = admin.Id,
                Title = "Junior .NET Developer",
                Description = "Build and maintain HR platform APIs and integrations.",
                Location = "Harare, Zimbabwe",
                EmploymentType = "Full-time",
                ExperienceLevel = "Junior",
                RequiredSkillsCsv = "c#,dotnet,sql,rest api,angular",
                SalaryMin = 65000,
                SalaryMax = 85000,
                IsOpen = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<User> EnsureSeedUserAsync(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string password,
        UserRole role)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (user is null)
        {
            user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = normalizedEmail,
                PhoneNumber = phoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
        }
        else
        {
            // Keep seed credentials deterministic for local/dev access.
            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Role = role;
            user.IsActive = true;
            user.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return user;
    }

    private async Task ReplaceLegacySeedAdminAsync(int replacementAdminId, string replacementAdminEmail)
    {
        var replacementEmail = replacementAdminEmail.Trim().ToLowerInvariant();
        var legacyEmails = new[] { "hr.admin@hrsytem.com", "hr.admin@hrsystem.com" };

        var legacyAdmins = await _dbContext.Users
            .Where(x => x.Role == UserRole.Admin
                        && x.Email != replacementEmail
                        && legacyEmails.Contains(x.Email))
            .ToListAsync();

        if (legacyAdmins.Count == 0)
        {
            return;
        }

        var legacyAdminIds = legacyAdmins.Select(x => x.Id).ToList();

        var postedJobs = await _dbContext.JobPostings
            .Where(x => legacyAdminIds.Contains(x.PostedByAdminId))
            .ToListAsync();

        foreach (var job in postedJobs)
        {
            job.PostedByAdminId = replacementAdminId;
            job.UpdatedAtUtc = DateTime.UtcNow;
        }

        var followUpNotes = await _dbContext.FollowUpNotes
            .Where(x => legacyAdminIds.Contains(x.AdminId))
            .ToListAsync();

        foreach (var note in followUpNotes)
        {
            note.AdminId = replacementAdminId;
        }

        _dbContext.Users.RemoveRange(legacyAdmins);
        await _dbContext.SaveChangesAsync();
    }

    private async Task EnsureDefaultPreferencesAsync()
    {
        var userIds = await _dbContext.Users.AsNoTracking().Select(x => x.Id).ToListAsync();
        var existingPrefUserIds = await _dbContext.UserPreferences.AsNoTracking().Select(x => x.UserId).ToListAsync();

        var missing = userIds.Except(existingPrefUserIds).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var userId in missing)
        {
            _dbContext.UserPreferences.Add(new UserPreference
            {
                UserId = userId,
                Theme = "light",
                AutoHideSidebar = true,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}
