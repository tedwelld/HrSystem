using System.ComponentModel.DataAnnotations;

namespace HrSystem.Core.Dtos.Auth;

public class RegisterRequestDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Role { get; set; } = "Candidate";

    [MaxLength(100)]
    public string? AdminInviteCode { get; set; }
}

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
