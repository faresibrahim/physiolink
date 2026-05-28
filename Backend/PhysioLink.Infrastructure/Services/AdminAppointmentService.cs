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

        public AdminAppointmentService(PhysioLinkDbContext dbContext, ICurrentClinicService currentClinicService)
        {
            _dbContext = dbContext;
            _currentClinicService = currentClinicService;
        }

        public async Task<PagedResult<AdminAppointmentDto>> GetAllAsync(int page, int pageSize, DateTime? from = null, DateTime? to = null, string? status = null)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

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

            var appointment = new Appointment(
                AppointmentStatus.Pending,
                appointmentTimeUtc,
                dto.PatientId,
                dto.TherapistName ?? string.Empty,
                clinicId,
                dto.Notes
            );
            appointment.Title = dto.Title;

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
    }
}
