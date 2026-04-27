using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Appointments;

namespace PhysioLink.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        public Task<PagedResult<AppointmentDto>> GetPatientAppointmentAsync(Guid patientId, int page, int pageSize);
        public Task<AppointmentDto> CreateAppointmentAsync(AppointmentRequestDto request);
    }
}