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

        // Moves one of the caller's appointments to another open slot. A confirmed
        // appointment gets a linked reschedule request (the original stays booked
        // until the clinic accepts it); a pending request is moved in place.
        Task<RescheduleResult> RescheduleAsync(Guid applicationUserId, Guid appointmentId, RescheduleAppointmentDto request, CancellationToken ct = default);

        // The caller's appointments incl. pending/rejected/expired/cancelled state.
        Task<List<PatientAppointmentDto>> GetMyAppointmentsAsync(Guid applicationUserId, CancellationToken ct = default);
    }
}
