using HrSystem.Core.Dtos.Auth;
using HrSystem.Core.Dtos.Users;
using HrSystem.Core.Interfaces;
using HrSystem.Core.Options;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HrSystem.Core.Services;

public class AuthService(
    HrSystemDbContext dbContext,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    INotificationService notificationService,
    ISnapshotService snapshotService) : IAuthService
{
    private readonly HrSystemDbContext _dbContext = dbContext;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ISnapshotService _snapshotService = snapshotService;

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var requestedRole = dto.Role.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase)
            ? UserRole.Admin
            : UserRole.Candidate;

        if (requestedRole == UserRole.Admin && dto.AdminInviteCode != _jwtOptions.AdminInviteCode)
        {
            throw new UnauthorizedAccessException("Invalid admin invite code.");
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Role = requestedRole,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        await _snapshotService.CaptureAsync(
            actorUserId: user.Id,
            source: "Auth",
            action: "Register",
            category: "User",
            relatedEntityId: user.Id,
            details: $"{user.Email} registered as {user.Role}.",
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

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
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

        return BuildAuthResponse(user);
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

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var (token, expiresAt) = _tokenService.CreateToken(user);
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
}
