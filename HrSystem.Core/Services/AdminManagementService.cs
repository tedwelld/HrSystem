using HrSystem.Core.Dtos.Admin;
using HrSystem.Core.Dtos.Users;
using HrSystem.Core.Interfaces;
using HrSystem.Data;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Core.Services;

public class AdminManagementService(
    HrSystemDbContext dbContext,
    IEmailSender emailSender,
    INotificationService notificationService,
    ISnapshotService snapshotService) : IAdminManagementService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ISnapshotService _snapshotService = snapshotService;

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Role)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => new AdminUserDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Role = x.Role.ToString(),
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task<AdminUserDto?> UpdateUserAsync(int adminUserId, int targetUserId, AdminUpdateUserDto dto)
    {
        await EnsureAdminAsync(adminUserId);

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == targetUserId);
        if (user is null)
        {
            return null;
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber.Trim();
        user.IsActive = dto.IsActive;
        user.Role = dto.Role.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase)
            ? UserRole.Admin
            : UserRole.Candidate;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: adminUserId,
            source: "Admin",
            action: "UpdateUser",
            category: "User",
            relatedEntityId: user.Id,
            details: $"Updated user '{user.Email}' profile/role/active state.",
            notifyAdmins: true);

        return new AdminUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    public async Task<List<AdminCompanyDto>> GetCompaniesAsync()
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new AdminCompanyDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                City = x.City,
                Country = x.Country,
                Phone = x.Phone,
                Email = x.Email,
                Description = x.Description
            })
            .ToListAsync();
    }

    public async Task<AdminCompanyDto?> UpdateCompanyAsync(int adminUserId, int companyId, AdminUpdateCompanyDto dto)
    {
        await EnsureAdminAsync(adminUserId);

        var company = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == companyId);
        if (company is null)
        {
            return null;
        }

        company.Name = dto.Name.Trim();
        company.Address = dto.Address.Trim();
        company.City = dto.City.Trim();
        company.Country = dto.Country.Trim();
        company.Phone = dto.Phone.Trim();
        company.Email = dto.Email.Trim();
        company.Description = dto.Description.Trim();
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: adminUserId,
            source: "Admin",
            action: "UpdateCompany",
            category: "Company",
            relatedEntityId: company.Id,
            details: $"Updated company '{company.Name}'.",
            notifyAdmins: true);

        return new AdminCompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Address = company.Address,
            City = company.City,
            Country = company.Country,
            Phone = company.Phone,
            Email = company.Email,
            Description = company.Description
        };
    }

    public async Task<AdminEmailSendResultDto> SendUserEmailAsync(int adminUserId, AdminSendUserEmailDto dto)
    {
        await EnsureAdminAsync(adminUserId);

        var explicitIds = dto.UserIds
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var query = _dbContext.Users.AsNoTracking().Where(x => x.IsActive);
        if (dto.IncludeAllCandidates)
        {
            query = query.Where(x => x.Role == UserRole.Candidate || explicitIds.Contains(x.Id));
        }
        else
        {
            query = query.Where(x => explicitIds.Contains(x.Id));
        }

        var recipients = await query.ToListAsync();
        var requested = recipients.Count;
        var success = 0;
        var failed = 0;

        foreach (var recipient in recipients)
        {
            var personalizedMessage = dto.Message
                .Replace("{{firstName}}", recipient.FirstName, StringComparison.OrdinalIgnoreCase)
                .Replace("{firstName}", recipient.FirstName, StringComparison.OrdinalIgnoreCase)
                .Replace("[firstName]", recipient.FirstName, StringComparison.OrdinalIgnoreCase)
                .Replace("{{lastName}}", recipient.LastName, StringComparison.OrdinalIgnoreCase)
                .Replace("{lastName}", recipient.LastName, StringComparison.OrdinalIgnoreCase)
                .Replace("[lastName]", recipient.LastName, StringComparison.OrdinalIgnoreCase)
                .Replace("{{email}}", recipient.Email, StringComparison.OrdinalIgnoreCase)
                .Replace("{email}", recipient.Email, StringComparison.OrdinalIgnoreCase)
                .Replace("[email]", recipient.Email, StringComparison.OrdinalIgnoreCase);

            var (ok, response) = await _emailSender.SendAsync(recipient.Email, dto.Subject.Trim(), personalizedMessage);
            if (ok)
            {
                success++;
                await _notificationService.CreateNotificationAsync(
                    userId: recipient.Id,
                    title: dto.Subject.Trim(),
                    message: personalizedMessage,
                    type: NotificationType.System,
                    relatedJobId: null,
                    sendEmail: false,
                    sendSms: false);
            }
            else
            {
                failed++;
                await _snapshotService.CaptureAsync(
                    actorUserId: adminUserId,
                    source: "AdminEmail",
                    action: "SendFailed",
                    category: "Communication",
                    relatedEntityId: recipient.Id,
                    details: $"Failed email to '{recipient.Email}': {response}",
                    notifyAdmins: true);
            }
        }

        await _snapshotService.CaptureAsync(
            actorUserId: adminUserId,
            source: "AdminEmail",
            action: "Send",
            category: "Communication",
            relatedEntityId: null,
            details: $"Email campaign sent. Requested={requested}, Success={success}, Failed={failed}.",
            notifyAdmins: true);

        return new AdminEmailSendResultDto
        {
            RequestedRecipients = requested,
            SuccessfullySent = success,
            Failed = failed
        };
    }

    private async Task EnsureAdminAsync(int userId)
    {
        var valid = await _dbContext.Users.AnyAsync(x => x.Id == userId && x.IsActive && x.Role == UserRole.Admin);
        if (!valid)
        {
            throw new UnauthorizedAccessException("Only active admin users can perform this action.");
        }
    }
}
