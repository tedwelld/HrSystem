namespace HrSystem.Core.Interfaces;

public interface ISmsSender
{
    Task<(bool Success, string Response)> SendAsync(string toPhoneNumber, string message);
}
