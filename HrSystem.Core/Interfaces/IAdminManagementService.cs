using HrSystem.Core.Dtos.Admin;
using HrSystem.Core.Dtos.Users;

namespace HrSystem.Core.Interfaces;

public interface IAdminManagementService
{
    Task<List<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto> CreateHrAdminAsync(int adminUserId, CreateAdminUserDto dto);
    Task<AdminUserDto?> UpdateUserAsync(int adminUserId, int targetUserId, AdminUpdateUserDto dto);
    Task<bool> DeleteUserAsync(int adminUserId, int targetUserId);
    Task<List<AdminCompanyDto>> GetCompaniesAsync();
    Task<AdminCompanyDto> CreateCompanyAsync(int adminUserId, CreateCompanyDto dto);
    Task<AdminCompanyDto?> UpdateCompanyAsync(int adminUserId, int companyId, AdminUpdateCompanyDto dto);
    Task<bool> DeleteCompanyAsync(int adminUserId, int companyId);
    Task<AdminEmailSendResultDto> SendUserEmailAsync(int adminUserId, AdminSendUserEmailDto dto);
}
