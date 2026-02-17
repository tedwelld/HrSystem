using HrSystem.Core.Interfaces;
using HrSystem.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrSystem.Core.Services;

public class SmsSender(IOptions<SmsOptions> options, ILogger<SmsSender> logger) : ISmsSender
{
    private readonly SmsOptions _options = options.Value;
    private readonly ILogger<SmsSender> _logger = logger;

    public Task<(bool Success, string Response)> SendAsync(string toPhoneNumber, string message)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("SMS skipped (provider disabled). To={Phone}, Message={Message}", toPhoneNumber, message);
            return Task.FromResult((true, "SMS provider disabled; simulated success."));
        }

        // Mock provider hook. Replace with Twilio/Azure Communication Services when credentials are available.
        _logger.LogInformation("SMS sent using {Provider}. From={From}, To={To}", _options.ProviderName, _options.FromNumber, toPhoneNumber);
        return Task.FromResult((true, "Sent"));
    }
}
