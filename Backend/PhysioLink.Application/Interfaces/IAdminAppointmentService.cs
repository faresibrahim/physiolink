using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Appointments;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminAppointmentService
    {
        Task<PagedResult<AdminAppointmentDto>> GetAllAsync(int page, int pageSize, DateTime? from = null, DateTime? to = null, string? status = null);
        Task<AdminAppointmentDto?> GetByIdAsync(Guid id);
        Task<AdminAppointmentDto> CreateAsync(CreateAppointmentDto dto);
        Task<AdminAppointmentDto?> UpdateAsync(Guid id, UpdateAppointmentDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
