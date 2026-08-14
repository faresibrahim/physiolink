using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Appointments;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminAppointmentService
    {
        Task<PagedResult<AdminAppointmentDto>> GetAllAsync(int page, int pageSize, DateTime? from = null, DateTime? to = null, string? status = null);

        // Archive of appointments whose due date has passed (all terminal after the
        // sweep runs: Completed / Rejected / Expired / CancelledByClinic). Optional
        // status + therapist-name filters, newest first.
        Task<PagedResult<AdminAppointmentDto>> GetHistoryAsync(int page, int pageSize, string? status = null, string? therapistName = null);

        Task<AdminAppointmentDto?> GetByIdAsync(Guid id);
        Task<AdminAppointmentDto> CreateAsync(CreateAppointmentDto dto);
        Task<AdminAppointmentDto?> UpdateAsync(Guid id, UpdateAppointmentDto dto);
        Task<bool> DeleteAsync(Guid id);

        // Slot-based decision flow (spec Phase 4). The requests queue runs the lazy
        // expiry sweep first so expired requests never appear in the live queue.
        Task<List<AppointmentRequestQueueDto>> GetRequestsAsync(Guid? therapistId, CancellationToken ct = default);
        Task<AppointmentActionOutcome> AcceptAsync(Guid appointmentId, CancellationToken ct = default);
        Task<AppointmentActionOutcome> RejectAsync(Guid appointmentId, CancellationToken ct = default);
        Task<AppointmentActionOutcome> CancelAsync(Guid appointmentId, CancellationToken ct = default);
    }
}
