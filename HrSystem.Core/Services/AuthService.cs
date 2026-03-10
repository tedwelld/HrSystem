using HrSystem.Core.Dtos.Auth;
using HrSystem.Core.Dtos.Users;
using HrSystem.Core.Interfaces;
using HrSystem.Core.Options;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace HrSystem.Core.Services;

public class AuthService(
    HrSystemDbContext dbContext,
    ITokenService tokenService,
    INotificationService notificationService,
    ISnapshotService snapshotService,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly ITokenService _tokenService = tokenService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ISnapshotService _snapshotService = snapshotService;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, string ipAddress, string userAgent)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var normalizedRole = string.IsNullOrWhiteSpace(dto.Role) ? "Candidate" : dto.Role.Trim();
        var role = ResolveRole(normalizedRole, dto.AdminInviteCode);

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.UserPreferences.Add(new UserPreference
        {
            UserId = user.Id,
            Theme = "light",
            AutoHideSidebar = true,
            UpdatedAtUtc = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: user.Id,
            source: "Auth",
            action: "Register",
            category: "User",
            relatedEntityId: user.Id,
            details: $"{user.Email} self-registered as {user.Role}.",
            notifyAdmins: true);

        var adminIds = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Role == UserRole.Admin && x.IsActive && x.Id != user.Id)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var adminId in adminIds)
        {
            await _notificationService.CreateNotificationAsync(
                userId: adminId,
                title: "New user registration",
                message: $"{user.FirstName} {user.LastName} registered as {user.Role}.",
                type: NotificationType.System,
                relatedJobId: null,
                sendEmail: false,
                sendSms: false);
        }

        return await BuildAuthResponseAsync(user, ipAddress, userAgent);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, string ipAddress, string userAgent)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail && x.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        await _snapshotService.CaptureAsync(
            actorUserId: user.Id,
            source: "Auth",
            action: "Login",
            category: "User",
            relatedEntityId: user.Id,
            details: $"{user.Email} logged in.",
            notifyAdmins: false);

        return await BuildAuthResponseAsync(user, ipAddress, userAgent);
    }

    public async Task LogoutAsync(int userId, string sessionToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return;
        }

        var tokenHash = SessionTokenHasher.Hash(sessionToken);
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RefreshTokenHash == tokenHash && x.RevokedAtUtc == null);

        if (session is null)
        {
            return;
        }

        session.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive)
            .Select(x => new UserProfileDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Role = x.Role.ToString()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserProfileDto?> UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null)
        {
            return null;
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: userId,
            source: "Auth",
            action: "UpdateProfile",
            category: "User",
            relatedEntityId: user.Id,
            details: $"{user.Email} updated own profile.",
            notifyAdmins: false);

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString()
        };
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user, string ipAddress, string userAgent)
    {
        var sessionToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var (token, expiresAt) = _tokenService.CreateToken(user, sessionToken);

        _dbContext.UserSessions.Add(new UserSession
        {
            UserId = user.Id,
            RefreshTokenHash = SessionTokenHasher.Hash(sessionToken),
            IpAddress = TrimOrDefault(ipAddress, 120, "unknown"),
            UserAgent = TrimOrDefault(userAgent, 600, "unknown"),
            ExpiresAtUtc = expiresAt,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAt,
            User = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString()
            }
        };
    }

    private UserRole ResolveRole(string role, string? adminInviteCode)
    {
        if (role.Equals("Candidate", StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.Candidate;
        }

        if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(adminInviteCode?.Trim(), _jwtOptions.AdminInviteCode, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("A valid admin invite code is required for admin registration.");
            }

            return UserRole.Admin;
        }

        throw new InvalidOperationException("Invalid role supplied.");
    }

    private static string TrimOrDefault(string value, int maxLength, string fallback)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
