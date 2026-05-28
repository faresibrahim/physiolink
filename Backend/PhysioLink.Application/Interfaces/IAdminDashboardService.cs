using PhysioLink.Application.DTOs;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardDto> GetAsync();
    }
}
