using HrSystem.Data.EntityModels;

namespace HrSystem.Core.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
