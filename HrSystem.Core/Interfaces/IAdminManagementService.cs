using HrSystem.Core.Dtos.Admin;
using HrSystem.Core.Dtos.Users;

namespace HrSystem.Core.Interfaces;

public interface IAdminManagementService
{
    Task<List<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto?> UpdateUserAsync(int adminUserId, int targetUserId, AdminUpdateUserDto dto);
    Task<List<AdminCompanyDto>> GetCompaniesAsync();
    Task<AdminCompanyDto?> UpdateCompanyAsync(int adminUserId, int companyId, AdminUpdateCompanyDto dto);
    Task<AdminEmailSendResultDto> SendUserEmailAsync(int adminUserId, AdminSendUserEmailDto dto);
}
