using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly PhysioLinkDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly ICurrentClinicService _currentClinic;

        // Dashboard stats shift as appointments/patients/assignments change all over
        // the app, so there is no single write to hang eviction on — a short TTL is
        // the right tool. 30s of staleness is invisible on an aggregate dashboard.
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        public AdminDashboardService(
            PhysioLinkDbContext dbContext,
            IMemoryCache cache,
            ICurrentClinicService currentClinic)
        {
            _dbContext = dbContext;
            _cache = cache;
            _currentClinic = currentClinic;
        }

        public async Task<DashboardDto> GetAsync()
        {
            // Key MUST include the clinic id: the counts below are clinic-scoped, so a
            // shared key would serve one clinic's totals to another (a tenant leak).
            var clinicId = _currentClinic.GetCurrentClinicId();
            var cacheKey = $"dashboard:{clinicId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                return await BuildDashboardAsync();
            })
            ?? await BuildDashboardAsync();
        }

        private async Task<DashboardDto> BuildDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var todayEnd = todayStart.AddDays(1);

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

            // Appointment is ClinicScopedEntity — global filter handles clinic scoping.
            // Only Confirmed appointments count — a pending request or a rejected/
            // cancelled/expired one isn't actually happening today.
            var appointmentsToday = await _dbContext.Appointments
                .CountAsync(a => a.AppointmentTime >= todayStart && a.AppointmentTime < todayEnd
                    && a.Status == AppointmentStatus.Confirmed);

            return new DashboardDto
            {
                PatientCount = patientCount,
                TherapistCount = therapistCount,
                ActiveAssignmentCount = activeAssignmentCount,
                AppointmentsToday = appointmentsToday
            };
        }
    }
}
