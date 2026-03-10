using HrSystem.Core.Interfaces;
using HrSystem.Core.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HrSystem.Core.Services;

public class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;
    private readonly ILogger<SmtpEmailSender> _logger = logger;
    private const string PlaceholderHost = "smtp.example.com";
    private const string PlaceholderPassword = "replace-me";
    private const string DefaultFromAddress = "noreply@hrsystem.com";

    public async Task<(bool Success, string Response)> SendAsync(string toEmail, string subject, string body)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email skipped (SMTP disabled). To={ToEmail}, Subject={Subject}", toEmail, subject);
            return (true, "SMTP disabled; simulated success.");
        }

        if (string.IsNullOrWhiteSpace(_options.Host) || _options.Host.Equals(PlaceholderHost, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "SMTP host is not configured.");
        }

        if (!string.IsNullOrWhiteSpace(_options.UserName)
            && (string.IsNullOrWhiteSpace(_options.Password) || _options.Password.Equals(PlaceholderPassword, StringComparison.Ordinal)))
        {
            return (false, "SMTP password is not configured.");
        }

        try
        {
            var message = new MimeMessage();
            var fromAddress = string.IsNullOrWhiteSpace(_options.FromAddress) ? DefaultFromAddress : _options.FromAddress.Trim();
            message.From.Add(new MailboxAddress(_options.FromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();

            var socketOptions = _options.UseSsl
                ? (_options.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
                : SecureSocketOptions.None;

            await client.ConnectAsync(_options.Host, _options.Port, socketOptions);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                await client.AuthenticateAsync(_options.UserName, _options.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return (true, "Sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send failed. To={ToEmail}, Subject={Subject}", toEmail, subject);
            return (false, ex.Message);
        }
    }
}
