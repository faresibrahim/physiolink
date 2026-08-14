using PhysioLink.Application.DTOs.Slots;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminSlotService
    {
        // Weekly toggle grid for one therapist. Returns null when the therapist is
        // not in the caller's clinic (spec 2.1 / D4).
        Task<SlotGridDto?> GetWeekGridAsync(Guid therapistId, DateTime weekStart);

        // Toggle a cell on. Idempotent — a duplicate create is a no-op (spec 2.2).
        Task<SlotWriteOutcome> CreateSlotAsync(Guid therapistId, DateTime scheduledAt);

        // Toggle a cell off. Refused (SlotIsLive) if the slot is Requested/Booked;
        // the guard is server-side, never the greyed UI (spec 2.3).
        Task<SlotWriteOutcome> DeleteSlotAsync(Guid therapistId, DateTime scheduledAt);
    }
}
