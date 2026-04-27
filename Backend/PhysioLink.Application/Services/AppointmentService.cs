using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }
        
        public async Task<PagedResult<AppointmentDto>> GetPatientAppointmentAsync(Guid patientId, int page, int pageSize)
        {
            return await _appointmentRepository.GetPatientAppointmentAsync(patientId, page, pageSize);
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(AppointmentRequestDto request)
        {
            return await _appointmentRepository.CreateAppointmentAsync(request);
        }
    }
}