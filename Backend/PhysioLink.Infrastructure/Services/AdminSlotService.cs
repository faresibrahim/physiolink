using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs.Slots;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    // Admin-side slot management (spec Phase 2). Tenant isolation leans on the global
    // clinic query filter for AppointmentSlot/Therapist; the clinic row itself is
    // fetched explicitly because Clinic is not a ClinicScopedEntity.
    public class AdminSlotService : IAdminSlotService
    {
        private readonly PhysioLinkDbContext _dbContext;
        private readonly ICurrentClinicService _currentClinicService;

        public AdminSlotService(PhysioLinkDbContext dbContext, ICurrentClinicService currentClinicService)
        {
            _dbContext = dbContext;
            _currentClinicService = currentClinicService;
        }

        public async Task<SlotGridDto?> GetWeekGridAsync(Guid therapistId, DateTime weekStart)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            // Therapists is clinic-scoped by the global filter, so this also enforces
            // that the therapist belongs to the caller's clinic (D4).
            var therapistExists = await _dbContext.Therapists
                .AnyAsync(t => t.TherapistId == therapistId);
            if (!therapistExists) return null;

            var (openHour, closeHour) = await GetOperatingHoursAsync(clinicId);

            var weekStartUtc = DateTime.SpecifyKind(weekStart.Date, DateTimeKind.Utc);
            var weekEndUtc = weekStartUtc.AddDays(7);

            // One round-trip for the whole week; index (TherapistId, ScheduledAt).
            var slots = await _dbContext.AppointmentSlots.AsNoTracking()
                .Where(s => s.TherapistId == therapistId
                            && s.ScheduledAt >= weekStartUtc
                            && s.ScheduledAt < weekEndUtc)
                .ToListAsync();

            var slotByStart = slots.ToDictionary(s => s.ScheduledAt);

            var cells = new List<SlotCellDto>();
            for (var day = 0; day < 7; day++)
            {
                for (var hour = openHour; hour < closeHour; hour++)
                {
                    var cellStart = weekStartUtc.AddDays(day).AddHours(hour);
                    var state = slotByStart.TryGetValue(cellStart, out var slot)
                        ? slot.Status.ToString()
                        : "Empty";

                    cells.Add(new SlotCellDto
                    {
                        ScheduledAt = cellStart,
                        DayIndex = day,
                        Hour = hour,
                        State = state,
                        SlotId = slot?.Id
                    });
                }
            }

            return new SlotGridDto
            {
                TherapistId = therapistId,
                WeekStart = weekStartUtc,
                OpenHour = openHour,
                CloseHour = closeHour,
                Cells = cells
            };
        }

        public async Task<List<TherapistSlotOptionDto>?> GetAvailableSlotsAsync(Guid therapistId, DateTime? from, DateTime? to)
        {
            // Therapists is clinic-scoped, so this also enforces clinic ownership (D4).
            var therapistExists = await _dbContext.Therapists
                .AnyAsync(t => t.TherapistId == therapistId);
            if (!therapistExists) return null;

            var now = DateTime.UtcNow;
            var fromUtc = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : now;
            var toUtc = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : now.AddDays(60);

            return await _dbContext.AppointmentSlots.AsNoTracking()
                .Where(s => s.TherapistId == therapistId
                            && s.Status == SlotStatus.Available
                            && s.ScheduledAt > now
                            && s.ScheduledAt >= fromUtc
                            && s.ScheduledAt <= toUtc)
                .OrderBy(s => s.ScheduledAt)
                .Select(s => new TherapistSlotOptionDto
                {
                    SlotId = s.Id,
                    ScheduledAt = s.ScheduledAt
                })
                .ToListAsync();
        }

        public async Task<SlotWriteOutcome> CreateSlotAsync(Guid therapistId, DateTime scheduledAt)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            var scheduledAtUtc = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc);

            // 1. Must be exactly on the hour (spec 2.2 step 1).
            if (scheduledAtUtc.Minute != 0 || scheduledAtUtc.Second != 0 || scheduledAtUtc.Millisecond != 0)
                return SlotWriteOutcome.NotOnTheHour;

            // 2. Therapist must belong to the caller's clinic (clinic-scoped query).
            var therapistExists = await _dbContext.Therapists
                .AnyAsync(t => t.TherapistId == therapistId);
            if (!therapistExists) return SlotWriteOutcome.TherapistNotFound;

            // 3. Within the clinic's operating window [OpenHour, CloseHour).
            var (openHour, closeHour) = await GetOperatingHoursAsync(clinicId);
            if (scheduledAtUtc.Hour < openHour || scheduledAtUtc.Hour >= closeHour)
                return SlotWriteOutcome.OutsideOperatingWindow;

            // 4. Idempotent create. The filtered unique index only blocks live rows,
            // so look across soft-deleted rows too (explicitly clinic-scoped) and
            // resurrect one instead of inserting a duplicate.
            var existing = await _dbContext.AppointmentSlots
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.ClinicId == clinicId
                                          && s.TherapistId == therapistId
                                          && s.ScheduledAt == scheduledAtUtc);

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.Status = SlotStatus.Available;
                    await _dbContext.SaveChangesAsync();
                }
                // Already live at this hour -> idempotent no-op.
                return SlotWriteOutcome.Ok;
            }

            _dbContext.AppointmentSlots.Add(new AppointmentSlot
            {
                ClinicId = clinicId,
                TherapistId = therapistId,
                ScheduledAt = scheduledAtUtc,
                Status = SlotStatus.Available
            });
            await _dbContext.SaveChangesAsync();

            return SlotWriteOutcome.Ok;
        }

        public async Task<SlotWriteOutcome> DeleteSlotAsync(Guid therapistId, DateTime scheduledAt)
        {
            var scheduledAtUtc = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Utc);

            // Clinic-scoped + non-deleted by the global filter.
            var slot = await _dbContext.AppointmentSlots
                .FirstOrDefaultAsync(s => s.TherapistId == therapistId && s.ScheduledAt == scheduledAtUtc);

            if (slot == null) return SlotWriteOutcome.SlotNotFound;

            // Server-side toggle-off guard (spec 2.3) — never trust the greyed UI.
            if (slot.Status != SlotStatus.Available) return SlotWriteOutcome.SlotIsLive;

            slot.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return SlotWriteOutcome.Ok;
        }

        private async Task<(int openHour, int closeHour)> GetOperatingHoursAsync(Guid clinicId)
        {
            // Clinic is not clinic-scoped, so fetch by id. Fall back to 8-18 if hours
            // were never seeded (matches the DbSeeder normalization).
            var hours = await _dbContext.Clinics.AsNoTracking()
                .Where(c => c.ClinicId == clinicId)
                .Select(c => new { c.OpenHour, c.CloseHour })
                .FirstOrDefaultAsync();

            if (hours == null || hours.CloseHour <= hours.OpenHour)
                return (8, 18);

            return (hours.OpenHour, hours.CloseHour);
        }
    }
}
