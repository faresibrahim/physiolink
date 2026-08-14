namespace PhysioLink.Application.DTOs.Slots
{
    // Slim slot projection for the patient booking list (spec 3.1). Deliberately
    // exposes no therapist internals — just the id to book and when.
    public class PatientSlotDto
    {
        public Guid SlotId { get; set; }
        public DateTime ScheduledAt { get; set; }

        // False once the slot is Requested or Booked. The patient list still shows
        // these so a taken time reads as "Booked" rather than silently disappearing;
        // only Available slots are actually bookable.
        public bool IsAvailable { get; set; }
    }
}
