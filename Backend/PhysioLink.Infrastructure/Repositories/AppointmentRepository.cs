using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly PhysioLinkDbContext _dbContext;

        public AppointmentRepository(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(AppointmentRequestDto request)
        {
            var appointment = new Appointment(
                AppointmentStatus.Pending,
                DateTime.SpecifyKind(request.AppointmentTime, DateTimeKind.Utc),
                request.PatientId,
                request.TherapistName, //To-do, wire therapistname from request in week 17
                Guid.Empty,// TODO: replace with ClinicId from JWT claim in Week 17
                request.Notes
            );

            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync();

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                TherapistName = appointment.TherapistName,
                Title = appointment.Title,
                Notes = appointment.Notes,
                AppointmentTime = appointment.AppointmentTime,
                AppointmentStatus = appointment.Status,
                PatientName = ""
            };
        }

        public async Task<PagedResult<AppointmentDto>> GetPatientAppointmentAsync(Guid patientId, int page, int pageSize)
        {
            var totalCount = await _dbContext.Appointments
                .Where(p => p.PatientId == patientId)
                .CountAsync();

            var items = await _dbContext.Appointments
                .Where(p => p.PatientId == patientId)
                .Include(p => p.Patient)
                .AsNoTracking()
                .Select(ea => new AppointmentDto
                {
                    AppointmentId = ea.AppointmentId,
                    PatientId = ea.PatientId,
                    TherapistName = ea.TherapistName,
                    Title = ea.Title,
                    Notes = ea.Notes,
                    AppointmentTime = ea.AppointmentTime,
                    AppointmentStatus = ea.Status,
                    PatientName = ea.Patient.FirstName + " " + ea.Patient.LastName,
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AppointmentDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }
    }
}
