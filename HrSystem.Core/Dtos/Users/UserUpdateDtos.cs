using System.ComponentModel.DataAnnotations;

namespace HrSystem.Core.Dtos.Users;

public class UpdateMyProfileDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class AdminUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class AdminUpdateUserDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Candidate";

    public bool IsActive { get; set; } = true;
}
