namespace HrSystem.Core.Interfaces;

public interface IEmailSender
{
    Task<(bool Success, string Response)> SendAsync(string toEmail, string subject, string body);
}
