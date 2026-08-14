using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.DTOs.Slots;

namespace PhysioLink.Application.Interfaces
{
    public interface IPatientSlotService
    {
        // Available slots for the caller's assigned therapist. Empty list (not an
        // error) when the patient is unassigned (spec D10 / 3.1).
        Task<List<PatientSlotDto>> GetMySlotsAsync(Guid applicationUserId, DateTime? from, DateTime? to, CancellationToken ct = default);

        // Consume-on-request: flips the slot Available -> Requested and creates the
        // pending appointment, race-safe via a conditional update (spec 3.2).
        Task<RequestSlotResult> RequestSlotAsync(Guid applicationUserId, RequestSlotDto request, CancellationToken ct = default);

        // The caller's appointments incl. pending/rejected/expired/cancelled state.
        Task<List<PatientAppointmentDto>> GetMyAppointmentsAsync(Guid applicationUserId, CancellationToken ct = default);
    }
}
