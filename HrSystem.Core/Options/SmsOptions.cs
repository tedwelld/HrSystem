namespace HrSystem.Core.Options;

public class SmsOptions
{
    public const string SectionName = "Sms";

    public bool Enabled { get; set; }
    public string ProviderName { get; set; } = "Mock";
    public string FromNumber { get; set; } = "+10000000000";
}
