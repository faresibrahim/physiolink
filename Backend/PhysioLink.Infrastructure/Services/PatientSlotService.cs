using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.DTOs.Slots;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    // Patient-side booking (spec Phase 3). All queries are clinic-scoped by the
    // global filter; the patient is resolved from the JWT identity (ApplicationUserId).
    public class PatientSlotService : IPatientSlotService
    {
        private readonly PhysioLinkDbContext _dbContext;
        private readonly ISlotExpiryService _expiryService;

        public PatientSlotService(PhysioLinkDbContext dbContext, ISlotExpiryService expiryService)
        {
            _dbContext = dbContext;
            _expiryService = expiryService;
        }

        public async Task<List<PatientSlotDto>> GetMySlotsAsync(Guid applicationUserId, DateTime? from, DateTime? to, CancellationToken ct = default)
        {
            var patient = await _dbContext.Patients.AsNoTracking()
                .Where(p => p.ApplicationUserId == applicationUserId)
                .Select(p => new { p.PatientId, p.TherapistId })
                .FirstOrDefaultAsync(ct);

            // Unassigned patient -> empty list, not an error (D10).
            if (patient?.TherapistId == null)
                return [];

            // Free any expired requests first so freshly-freed slots show up (3.1 step 3).
            await _expiryService.SweepAsync(ct);

            var now = DateTime.UtcNow;
            var fromUtc = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : now;
            var toUtc = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : now.AddDays(60);

            // Include taken slots (Requested/Booked) too — the patient UI keeps them
            // visible as "Booked" instead of removing them. Only Available slots are
            // bookable, flagged via IsAvailable.
            return await _dbContext.AppointmentSlots.AsNoTracking()
                .Where(s => s.TherapistId == patient.TherapistId
                            && s.ScheduledAt > now
                            && s.ScheduledAt >= fromUtc
                            && s.ScheduledAt <= toUtc)
                .OrderBy(s => s.ScheduledAt)
                .Select(s => new PatientSlotDto
                {
                    SlotId = s.Id,
                    ScheduledAt = s.ScheduledAt,
                    IsAvailable = s.Status == SlotStatus.Available
                })
                .ToListAsync(ct);
        }

        public async Task<RequestSlotResult> RequestSlotAsync(Guid applicationUserId, RequestSlotDto request, CancellationToken ct = default)
        {
            var patient = await _dbContext.Patients.AsNoTracking()
                .Where(p => p.ApplicationUserId == applicationUserId)
                .Select(p => new { p.PatientId, p.ClinicId, p.TherapistId })
                .FirstOrDefaultAsync(ct);

            if (patient == null)
                return RequestSlotResult.PatientNotFound();

            // Unassigned patient cannot book (D10) — treat as a conflict.
            if (patient.TherapistId == null)
                return RequestSlotResult.Conflict();

            var now = DateTime.UtcNow;

            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

            // Conditional update: only succeeds while the slot is still Available and
            // belongs to the patient's therapist. Atomic — the loser sees 0 rows
            // (spec 3.2). Global query filters (clinic + IsDeleted) still apply.
            var rows = await _dbContext.AppointmentSlots
                .Where(s => s.Id == request.SlotId
                            && s.TherapistId == patient.TherapistId
                            && s.Status == SlotStatus.Available
                            && s.ScheduledAt > now)
                .ExecuteUpdateAsync(set => set.SetProperty(s => s.Status, SlotStatus.Requested), ct);

            if (rows == 0)
            {
                await tx.RollbackAsync(ct);
                return RequestSlotResult.Conflict();
            }

            var slotScheduledAt = await _dbContext.AppointmentSlots.AsNoTracking()
                .Where(s => s.Id == request.SlotId)
                .Select(s => s.ScheduledAt)
                .FirstAsync(ct);

            var therapistName = await _dbContext.Therapists.AsNoTracking()
                .Where(t => t.TherapistId == patient.TherapistId)
                .Select(t => t.FirstName + " " + t.LastName)
                .FirstAsync(ct);

            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                ClinicId = patient.ClinicId,
                TherapistId = patient.TherapistId.Value,
                TherapistName = therapistName,          // snapshot at request time (1.4)
                AppointmentSlotId = request.SlotId,
                AppointmentTime = slotScheduledAt,
                Title = string.IsNullOrWhiteSpace(request.Type) ? "Appointment Request" : request.Type!,
                Notes = request.Notes,
                Status = AppointmentStatus.Requested    // CreatedAt is the request time
            };

            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return RequestSlotResult.Created(ToDto(appointment));
        }

        public async Task<RescheduleResult> RescheduleAsync(Guid applicationUserId, Guid appointmentId, RescheduleAppointmentDto request, CancellationToken ct = default)
        {
            var patient = await _dbContext.Patients.AsNoTracking()
                .Where(p => p.ApplicationUserId == applicationUserId)
                .Select(p => new { p.PatientId, p.ClinicId, p.TherapistId })
                .FirstOrDefaultAsync(ct);

            if (patient == null)
                return RescheduleResult.Of(RescheduleOutcome.NotFound);

            // Settle stale requests first so an expired one can't be moved.
            await _expiryService.SweepAsync(ct);

            // Ownership is part of the lookup: another patient's id reads as 404.
            var appointment = await _dbContext.Appointments
                .Include(a => a.AppointmentSlot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.PatientId == patient.PatientId, ct);

            if (appointment == null)
                return RescheduleResult.Of(RescheduleOutcome.NotFound);

            if (appointment.Status is not (AppointmentStatus.Confirmed or AppointmentStatus.Requested))
                return RescheduleResult.Of(RescheduleOutcome.NotReschedulable);

            var now = DateTime.UtcNow;
            if (appointment.AppointmentTime - now < RescheduleResult.Cutoff)
                return RescheduleResult.Of(RescheduleOutcome.TooLate);

            if (appointment.Status == AppointmentStatus.Confirmed)
            {
                // One open reschedule request per confirmed session; the patient
                // moves that request instead of stacking another.
                var pending = await _dbContext.Appointments.AnyAsync(a =>
                    a.RescheduleOfAppointmentId == appointment.AppointmentId
                    && a.Status == AppointmentStatus.Requested, ct);
                if (pending)
                    return RescheduleResult.Of(RescheduleOutcome.NotReschedulable);
            }

            if (patient.TherapistId == null || request.SlotId == appointment.AppointmentSlotId)
                return RescheduleResult.Of(RescheduleOutcome.SlotUnavailable);

            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

            // Same race-safe claim as RequestSlotAsync (spec 3.2).
            var rows = await _dbContext.AppointmentSlots
                .Where(s => s.Id == request.SlotId
                            && s.TherapistId == patient.TherapistId
                            && s.Status == SlotStatus.Available
                            && s.ScheduledAt > now)
                .ExecuteUpdateAsync(set => set.SetProperty(s => s.Status, SlotStatus.Requested), ct);

            if (rows == 0)
            {
                await tx.RollbackAsync(ct);
                return RescheduleResult.Of(RescheduleOutcome.SlotUnavailable);
            }

            // Tracked, loaded after the claim so it carries the new status.
            var newSlot = await _dbContext.AppointmentSlots.FirstAsync(s => s.Id == request.SlotId, ct);

            if (appointment.Status == AppointmentStatus.Requested)
            {
                // Still undecided: move the request itself. The old slot goes back
                // in the pool and the request re-enters the queue as of now, so the
                // 48h decision window restarts for the new time.
                if (appointment.AppointmentSlot is { Status: SlotStatus.Requested })
                    appointment.AppointmentSlot.Status = SlotStatus.Available;

                appointment.AppointmentSlot = newSlot; // FK follows the navigation
                appointment.AppointmentTime = newSlot.ScheduledAt;
                appointment.CreatedAt = now;

                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return RescheduleResult.Of(RescheduleOutcome.Moved, ToDto(appointment));
            }

            // Confirmed: keep it booked and ask for the new time alongside it.
            var therapistName = await _dbContext.Therapists.AsNoTracking()
                .Where(t => t.TherapistId == patient.TherapistId)
                .Select(t => t.FirstName + " " + t.LastName)
                .FirstAsync(ct);

            var replacement = new Appointment
            {
                PatientId = patient.PatientId,
                ClinicId = patient.ClinicId,
                TherapistId = patient.TherapistId.Value,
                TherapistName = therapistName,
                AppointmentSlotId = newSlot.Id,
                AppointmentTime = newSlot.ScheduledAt,
                Title = appointment.Title,
                Notes = appointment.Notes,
                Status = AppointmentStatus.Requested,
                RescheduleOfAppointmentId = appointment.AppointmentId
            };

            _dbContext.Appointments.Add(replacement);
            await _dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return RescheduleResult.Of(RescheduleOutcome.Requested, ToDto(replacement));
        }

        public async Task<List<PatientAppointmentDto>> GetMyAppointmentsAsync(Guid applicationUserId, CancellationToken ct = default)
        {
            var patientId = await _dbContext.Patients.AsNoTracking()
                .Where(p => p.ApplicationUserId == applicationUserId)
                .Select(p => (Guid?)p.PatientId)
                .FirstOrDefaultAsync(ct);

            if (patientId == null)
                return [];

            // Sweep so a request left past its window reads as Expired, not Requested.
            await _expiryService.SweepAsync(ct);

            var appointments = await _dbContext.Appointments.AsNoTracking()
                // A replaced appointment is history the patient doesn't need: its
                // successor is in the list. Hiding it also keeps older app builds,
                // which don't know the Rescheduled status, from failing to parse.
                .Where(a => a.PatientId == patientId && a.Status != AppointmentStatus.Rescheduled)
                .OrderByDescending(a => a.AppointmentTime)
                .ToListAsync(ct);

            return appointments.Select(ToDto).ToList();
        }

        private static PatientAppointmentDto ToDto(Appointment a) => new()
        {
            AppointmentId = a.AppointmentId,
            ScheduledAt = a.AppointmentTime,
            TherapistName = a.TherapistName,
            Type = a.Title,
            Notes = a.Notes,
            Status = a.Status.ToString(),
            StatusLabel = a.Status.ToLabel(),
            SlotId = a.AppointmentSlotId,
            RescheduleOfAppointmentId = a.RescheduleOfAppointmentId
        };
    }
}
