using PhysioLink.Domain.Enums;

namespace PhysioLink.Domain.Entities
{
    // The bookable primitive (spec D1/D2). Clinic-scoped so it picks up the global
    // clinic query filter for tenant isolation. A slot is defined by its start time
    // only — there is no stored duration (D9): the grid is hourly and the session
    // is ~45 min inside the hour.
    public class AppointmentSlot : ClinicScopedEntity
    {
        public Guid Id { get; set; }

        // D1 — a slot cannot exist without a therapist.
        public Guid TherapistId { get; set; }
        public Therapist? Therapist { get; set; }

        // Start of the hour block, always stored as UTC (timestamptz).
        public DateTime ScheduledAt { get; set; }

        public SlotStatus Status { get; set; } = SlotStatus.Available;

        public AppointmentSlot()
        {
            Id = Guid.NewGuid();
        }
    }
}
