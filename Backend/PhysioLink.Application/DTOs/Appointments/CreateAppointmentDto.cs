namespace PhysioLink.Application.DTOs.Appointments
{
    public class CreateAppointmentDto
    {
        public required string Title { get; set; }
        public string? Notes { get; set; }
        public Guid PatientId { get; set; }
        public string? TherapistName { get; set; }
        public DateTime AppointmentTime { get; set; }

        // When set, the appointment is booked against this specific open slot: the
        // slot is claimed (Available -> Booked), the appointment is Confirmed, and its
        // time/therapist come from the slot. The two write paths (manual booking and
        // the slot grid) can no longer double-book. Null keeps the legacy free-time
        // behaviour for any caller that still sends a raw AppointmentTime.
        public Guid? SlotId { get; set; }
    }
}
