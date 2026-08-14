using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class AdminAppointmentService : IAdminAppointmentService
    {
        private readonly PhysioLinkDbContext _dbContext;
        private readonly ICurrentClinicService _currentClinicService;
        private readonly ISlotExpiryService _expiryService;

        public AdminAppointmentService(PhysioLinkDbContext dbContext, ICurrentClinicService currentClinicService, ISlotExpiryService expiryService)
        {
            _dbContext = dbContext;
            _currentClinicService = currentClinicService;
            _expiryService = expiryService;
        }

        public async Task<PagedResult<AdminAppointmentDto>> GetAllAsync(int page, int pageSize, DateTime? from = null, DateTime? to = null, string? status = null)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            // Materialize time-driven transitions first so the board reflects reality:
            // past-due requests read as Expired and past-due confirmed as Completed
            // (the latter then falls out of any upcoming window the caller passes).
            await _expiryService.SweepAsync();

            var baseQuery = _dbContext.Appointments.AsNoTracking()
                .Where(a => a.ClinicId == clinicId);

            if (from.HasValue)
                baseQuery = baseQuery.Where(a => a.AppointmentTime >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));

            if (to.HasValue)
                baseQuery = baseQuery.Where(a => a.AppointmentTime < DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
                baseQuery = baseQuery.Where(a => a.Status == parsedStatus);

            var query = baseQuery.OrderBy(a => a.AppointmentTime).Select(a => new AdminAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    Title = a.Title,
                    Notes = a.Notes,
                    PatientId = a.PatientId,
                    PatientName = _dbContext.Patients
                        .Where(p => p.PatientId == a.PatientId)
                        .Select(p => p.FirstName + " " + p.LastName)
                        .FirstOrDefault() ?? string.Empty,
                    TherapistName = a.TherapistName,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status
                });

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AdminAppointmentDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<PagedResult<AdminAppointmentDto>> GetHistoryAsync(int page, int pageSize, string? status = null, string? therapistName = null)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            // Settle time-driven transitions so past-due confirmed/requested rows are
            // already Completed/Expired before we read the archive.
            await _expiryService.SweepAsync();

            // History = every appointment whose due date is in the past. Using the same
            // date boundary the board uses keeps the two views complementary: an
            // appointment shows on the board OR in History, never both.
            var todayUtc = DateTime.UtcNow.Date;

            var baseQuery = _dbContext.Appointments.AsNoTracking()
                .Where(a => a.ClinicId == clinicId && a.AppointmentTime < todayUtc);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
                baseQuery = baseQuery.Where(a => a.Status == parsedStatus);

            if (!string.IsNullOrEmpty(therapistName))
                baseQuery = baseQuery.Where(a => a.TherapistName == therapistName);

            var query = baseQuery.OrderByDescending(a => a.AppointmentTime).Select(a => new AdminAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    Title = a.Title,
                    Notes = a.Notes,
                    PatientId = a.PatientId,
                    PatientName = _dbContext.Patients
                        .Where(p => p.PatientId == a.PatientId)
                        .Select(p => p.FirstName + " " + p.LastName)
                        .FirstOrDefault() ?? string.Empty,
                    TherapistName = a.TherapistName,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status
                });

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AdminAppointmentDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<AdminAppointmentDto?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Appointments.AsNoTracking()
                .Where(a => a.AppointmentId == id)
                .Select(a => new AdminAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    Title = a.Title,
                    Notes = a.Notes,
                    PatientId = a.PatientId,
                    PatientName = _dbContext.Patients
                        .Where(p => p.PatientId == a.PatientId)
                        .Select(p => p.FirstName + " " + p.LastName)
                        .FirstOrDefault() ?? string.Empty,
                    TherapistName = a.TherapistName,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AdminAppointmentDto> CreateAsync(CreateAppointmentDto dto)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            var patientName = await _dbContext.Patients.AsNoTracking()
                .Where(p => p.PatientId == dto.PatientId)
                .Select(p => p.FirstName + " " + p.LastName)
                .FirstOrDefaultAsync() ?? string.Empty;

            var appointmentTimeUtc = DateTime.SpecifyKind(dto.AppointmentTime, DateTimeKind.Utc);

            // TherapistId is a non-null FK now (spec 1.4). Resolve one within the
            // clinic: prefer a name match, otherwise fall back to the patient's
            // assigned therapist so the appointment always references a real row.
            var therapist = await _dbContext.Therapists.AsNoTracking()
                .Where(t => !string.IsNullOrEmpty(dto.TherapistName)
                            && (t.FirstName + " " + t.LastName) == dto.TherapistName)
                .Select(t => new { t.TherapistId, t.FirstName, t.LastName })
                .FirstOrDefaultAsync();

            if (therapist == null)
            {
                var assignedTherapistId = await _dbContext.Patients.AsNoTracking()
                    .Where(p => p.PatientId == dto.PatientId)
                    .Select(p => p.TherapistId)
                    .FirstOrDefaultAsync();

                therapist = await _dbContext.Therapists.AsNoTracking()
                    .Where(t => assignedTherapistId != null && t.TherapistId == assignedTherapistId)
                    .Select(t => new { t.TherapistId, t.FirstName, t.LastName })
                    .FirstOrDefaultAsync();
            }

            if (therapist == null)
                throw new InvalidOperationException("No therapist could be resolved for this appointment.");

            var appointment = new Appointment
            {
                Status = AppointmentStatus.Requested,
                AppointmentTime = appointmentTimeUtc,
                PatientId = dto.PatientId,
                TherapistId = therapist.TherapistId,
                TherapistName = therapist.FirstName + " " + therapist.LastName,
                ClinicId = clinicId,
                Notes = dto.Notes,
                Title = dto.Title
            };

            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync();

            return new AdminAppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                Title = appointment.Title,
                Notes = appointment.Notes,
                PatientId = appointment.PatientId,
                PatientName = patientName,
                TherapistName = appointment.TherapistName,
                AppointmentTime = appointment.AppointmentTime,
                Status = appointment.Status
            };
        }

        public async Task<AdminAppointmentDto?> UpdateAsync(Guid id, UpdateAppointmentDto dto)
        {
            var appointment = await _dbContext.Appointments
                .SingleOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return null;

            var patientName = await _dbContext.Patients.AsNoTracking()
                .Where(p => p.PatientId == appointment.PatientId)
                .Select(p => p.FirstName + " " + p.LastName)
                .FirstOrDefaultAsync() ?? string.Empty;

            appointment.Title = dto.Title;
            appointment.Notes = dto.Notes;
            appointment.TherapistName = dto.TherapistName ?? string.Empty;
            appointment.AppointmentTime = DateTime.SpecifyKind(dto.AppointmentTime, DateTimeKind.Utc);
            appointment.Status = dto.Status;

            await _dbContext.SaveChangesAsync();

            return new AdminAppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                Title = appointment.Title,
                Notes = appointment.Notes,
                PatientId = appointment.PatientId,
                PatientName = patientName,
                TherapistName = appointment.TherapistName,
                AppointmentTime = appointment.AppointmentTime,
                Status = appointment.Status
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var appointment = await _dbContext.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return false;

            appointment.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<AppointmentRequestQueueDto>> GetRequestsAsync(Guid? therapistId, CancellationToken ct = default)
        {
            // Expired requests must never appear in the live queue (spec 4.1).
            await _expiryService.SweepAsync(ct);

            var query = _dbContext.Appointments.AsNoTracking()
                .Where(a => a.Status == AppointmentStatus.Requested);

            if (therapistId.HasValue)
                query = query.Where(a => a.TherapistId == therapistId.Value);

            return await query
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new AppointmentRequestQueueDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    PatientName = _dbContext.Patients
                        .Where(p => p.PatientId == a.PatientId)
                        .Select(p => p.FirstName + " " + p.LastName)
                        .FirstOrDefault() ?? string.Empty,
                    TherapistId = a.TherapistId,
                    TherapistName = a.TherapistName,
                    ScheduledAt = a.AppointmentTime,
                    Type = a.Title,
                    Notes = a.Notes,
                    RequestedAt = a.CreatedAt,
                    Status = a.Status
                })
                .ToListAsync(ct);
        }

        public async Task<AppointmentActionOutcome> AcceptAsync(Guid appointmentId, CancellationToken ct = default)
        {
            // Accept -> Confirmed / slot Booked (spec 4.2).
            return await TransitionAsync(
                appointmentId,
                required: AppointmentStatus.Requested,
                next: AppointmentStatus.Confirmed,
                slotNext: SlotStatus.Booked,
                ct);
        }

        public async Task<AppointmentActionOutcome> RejectAsync(Guid appointmentId, CancellationToken ct = default)
        {
            // Reject -> slot back in the pool (spec 4.3 / D6).
            return await TransitionAsync(
                appointmentId,
                required: AppointmentStatus.Requested,
                next: AppointmentStatus.Rejected,
                slotNext: SlotStatus.Available,
                ct);
        }

        public async Task<AppointmentActionOutcome> CancelAsync(Guid appointmentId, CancellationToken ct = default)
        {
            // Cancel a confirmed booking -> CancelledByClinic / slot freed (spec 4.4 / D8).
            // Default to freeing the slot; taking the therapist off entirely is a
            // separate concern left to the toggle-off grid.
            return await TransitionAsync(
                appointmentId,
                required: AppointmentStatus.Confirmed,
                next: AppointmentStatus.CancelledByClinic,
                slotNext: SlotStatus.Available,
                ct);
        }

        // Shared guarded transition: load the appointment (clinic-scoped) with its
        // slot, verify the required current state, then flip appointment + slot in one
        // SaveChanges (a single transaction).
        private async Task<AppointmentActionOutcome> TransitionAsync(
            Guid appointmentId,
            AppointmentStatus required,
            AppointmentStatus next,
            SlotStatus slotNext,
            CancellationToken ct)
        {
            var appointment = await _dbContext.Appointments
                .Include(a => a.AppointmentSlot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct);

            if (appointment == null) return AppointmentActionOutcome.NotFound;
            if (appointment.Status != required) return AppointmentActionOutcome.InvalidState;

            appointment.Status = next;
            if (appointment.AppointmentSlot != null)
                appointment.AppointmentSlot.Status = slotNext;

            await _dbContext.SaveChangesAsync(ct);
            return AppointmentActionOutcome.Ok;
        }
    }
}
