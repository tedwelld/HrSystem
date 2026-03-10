using System.Security.Cryptography;
using System.Text;

namespace HrSystem.Core.Services;

public static class SessionTokenHasher
{
    public static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
