using HrSystem.Core.Dtos.Dashboard;

namespace HrSystem.Core.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<CandidateDashboardDto> GetCandidateDashboardAsync(int userId);
}
