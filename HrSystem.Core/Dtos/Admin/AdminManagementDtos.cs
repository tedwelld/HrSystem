using System.ComponentModel.DataAnnotations;

namespace HrSystem.Core.Dtos.Admin;

public class AdminCompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AdminUpdateCompanyDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}

public class AdminSendUserEmailDto
{
    public List<int> UserIds { get; set; } = [];
    public bool IncludeAllCandidates { get; set; }

    [Required, MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}

public class AdminEmailSendResultDto
{
    public int RequestedRecipients { get; set; }
    public int SuccessfullySent { get; set; }
    public int Failed { get; set; }
}
