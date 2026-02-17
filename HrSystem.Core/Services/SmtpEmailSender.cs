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

    public async Task<(bool Success, string Response)> SendAsync(string toEmail, string subject, string body)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email skipped (SMTP disabled). To={ToEmail}, Subject={Subject}", toEmail, subject);
            return (true, "SMTP disabled; simulated success.");
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

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
