namespace HrSystem.Core.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "HrSystem.Api";
    public string Audience { get; set; } = "HrSystem.Web";
    public string SecretKey { get; set; } = "REPLACE_WITH_A_LONG_SECRET_KEY_1234567890";
    public int AccessTokenMinutes { get; set; } = 240;
    public string AdminInviteCode { get; set; } = "HRADMIN2026";
}
