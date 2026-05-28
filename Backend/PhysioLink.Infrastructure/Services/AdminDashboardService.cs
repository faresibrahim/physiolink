using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly PhysioLinkDbContext _dbContext;

        public AdminDashboardService(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardDto> GetAsync()
        {
            var now = DateTime.UtcNow;
            var weekEnd = now.AddDays(7);

            // Sequential awaits — EF Core DbContext does not support concurrent operations
            var patientCount = await _dbContext.Patients
                .CountAsync(p => p.IsActive);

            var therapistCount = await _dbContext.Therapists
                .CountAsync(t => t.IsActive);

            // ExerciseAssignment has no ClinicId — scope to clinic via Patient subquery
            // Global filter on Patients handles clinic scoping automatically
            var activeAssignmentCount = await _dbContext.ExerciseAssignments
                .CountAsync(ea => ea.Status == AssignmentStatus.Active
                    && _dbContext.Patients.Any(p => p.PatientId == ea.PatientId));

            // Appointment is ClinicScopedEntity — global filter handles clinic scoping
            var appointmentsThisWeek = await _dbContext.Appointments
                .CountAsync(a => a.AppointmentTime >= now && a.AppointmentTime <= weekEnd);

            return new DashboardDto
            {
                PatientCount = patientCount,
                TherapistCount = therapistCount,
                ActiveAssignmentCount = activeAssignmentCount,
                AppointmentsThisWeek = appointmentsThisWeek
            };
        }
    }
}
