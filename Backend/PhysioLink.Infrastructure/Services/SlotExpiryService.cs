using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class SlotExpiryService : ISlotExpiryService
    {
        private readonly PhysioLinkDbContext _dbContext;

        public SlotExpiryService(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SweepAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var todayUtc = now.Date;

            // A Requested appointment is expired when now >= min(CreatedAt + 48h,
            // slot start). The min is expressed as "either condition frees it"
            // (spec 3.3). Appointments is clinic-scoped by the global filter, so this
            // only ever touches the current clinic's rows.
            var expired = await _dbContext.Appointments
                .Where(a => a.Status == AppointmentStatus.Requested)
                .Where(a => now >= a.CreatedAt.AddHours(48) || now >= a.AppointmentTime)
                .Include(a => a.AppointmentSlot)
                .ToListAsync(ct);

            foreach (var appt in expired)
            {
                appt.Status = AppointmentStatus.Expired;
                if (appt.AppointmentSlot is { Status: SlotStatus.Requested })
                    appt.AppointmentSlot.Status = SlotStatus.Available; // free it
            }

            // Auto-complete Confirmed appointments once their due date has passed, so
            // they drop off the active board and land in History instead of lingering
            // as "Confirmed" forever (nothing "vanishes"). A DATE boundary is used
            // (not the exact instant): a confirmed session stays on the board for the
            // whole of its day and only completes once that date is in the past — the
            // same boundary the board/History views split on.
            var completed = await _dbContext.Appointments
                .Where(a => a.Status == AppointmentStatus.Confirmed)
                .Where(a => a.AppointmentTime < todayUtc)
                .ToListAsync(ct);

            foreach (var appt in completed)
                appt.Status = AppointmentStatus.Completed;

            if (expired.Count > 0 || completed.Count > 0)
                await _dbContext.SaveChangesAsync(ct);

            return expired.Count;
        }
    }
}
