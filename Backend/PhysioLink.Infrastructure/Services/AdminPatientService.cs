using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.Exceptions;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class AdminPatientService : IAdminPatientService
    {
        private readonly PhysioLinkDbContext _dbContext;
        private readonly ICurrentClinicService _currentClinicService;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public AdminPatientService(
            PhysioLinkDbContext dbContext,
            ICurrentClinicService currentClinicService,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _dbContext = dbContext;
            _currentClinicService = currentClinicService;
            _passwordHasher = passwordHasher;
        }

        public async Task<PagedResult<PatientDto>> GetAllAsync(int page, int pageSize, Guid? therapistId, string? search = null)
        {
            // Apply filters on the entity BEFORE projection - EF Core composes cleaner SQL
            // and there's no risk of the filter being lost in a LEFT JOIN projection.
            var query = _dbContext.Patients.AsNoTracking();

            if (therapistId.HasValue)
                query = query.Where(p => p.TherapistId == therapistId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(lowered) ||
                    p.LastName.ToLower().Contains(lowered) ||
                    (p.FirstName + " " + p.LastName).ToLower().Contains(lowered));
            }

            // Stable ordering is required for correct pagination.
            var ordered = query
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.PatientId);

            // Use a correlated subquery for the therapist name (same pattern as GetByIdAsync).
            // This avoids the LEFT JOIN + DefaultIfEmpty combination that confused EF Core's
            // translator when a Where was applied to the projected IQueryable<PatientDto>.
            var projection = ordered.Select(p => new PatientDto
            {
                PatientId   = p.PatientId,
                FirstName   = p.FirstName,
                LastName    = p.LastName,
                Username    = p.Username,
                Email       = p.Email,
                PhoneNumber = p.PhoneNumber,
                Diagnosis   = p.Diagnosis,
                IsActive    = p.IsActive,
                TherapistId = p.TherapistId,
                TherapistName = p.TherapistId != null
                    ? _dbContext.Therapists
                        .Where(t => t.TherapistId == p.TherapistId)
                        .Select(t => t.FirstName + " " + t.LastName)
                        .FirstOrDefault()
                    : null
            });

            var totalCount = await projection.CountAsync();

            var items = await projection
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PatientDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<PatientDetailDto?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Patients.AsNoTracking()
                .Where(p => p.PatientId == id)
                .Select(p => new PatientDetailDto
                {
                    PatientId = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Username = p.Username,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Diagnosis = p.Diagnosis,
                    IsActive = p.IsActive,
                    TherapistName = p.TherapistId != null
                        ? _dbContext.Therapists
                            .Where(t => t.TherapistId == p.TherapistId)
                            .Select(t => t.FirstName + " " + t.LastName)
                            .FirstOrDefault()
                        : null,
                    ClinicName = _dbContext.Clinics
                        .Where(c => c.ClinicId == p.ClinicId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? string.Empty,
                    Appointments = _dbContext.Appointments
                        .Where(a => a.PatientId == p.PatientId)
                        .Select(a => new AppointmentSummaryDto
                        {
                            AppointmentId   = a.AppointmentId,
                            Title           = a.Title,
                            AppointmentTime = a.AppointmentTime,
                            Status          = a.Status,
                            Notes           = a.Notes,
                            PatientId       = a.PatientId,
                            TherapistName   = a.TherapistName,
                            PatientName     = a.Patient.FirstName + " " + a.Patient.LastName
                        })
                        .ToList(),
                    Exercises = _dbContext.ExerciseAssignments
                        .Where(ea => ea.PatientId == p.PatientId)
                        .Select(ea => new AssignmentSummaryDto
                        {
                            ExerciseAssignmentId = ea.ExerciseAssignmentId,
                            ExerciseName = ea.Exercise.Name,
                            Sets = ea.Sets,
                            Reps = ea.Reps,
                            DurationMinutes = ea.DurationMinutes,
                            Feedback = ea.Feedback,
                            Status = ea.Status
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto createPatientDto)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            // Store usernames canonically lower-cased so the (case-sensitive) unique
            // index IX_Users_Username and the login lookup agree with this guard. Without
            // canonical storage, "Sarah" and "sarah" would be distinct to the index
            // but duplicates to this guard.
            // Guard against duplicate active accounts. The global IsDeleted query filter
            // means this only matches *active* users, so a username freed by a previous
            // deactivation can be reused.
            var username = createPatientDto.Username.Trim().ToLowerInvariant();
            var usernameInUse = await _dbContext.Users
                .AnyAsync(u => u.Username == username);
            if (usernameInUse)
                throw new UsernameInUseException(username);

            var email = string.IsNullOrWhiteSpace(createPatientDto.Email)
                ? null
                : createPatientDto.Email.Trim().ToLowerInvariant();

            var temporaryPassword = GeneratePassword();

            var user = new ApplicationUser(
                createPatientDto.FirstName,
                createPatientDto.LastName,
                email,
                passwordHash: string.Empty,
                role: "Patient",
                clinicId
            );
            user.Username = username;
            user.PasswordHash = _passwordHasher.HashPassword(user, temporaryPassword);
            // Force the patient to replace the temporary password on first login.
            user.MustChangePassword = true;

            var patient = new Patient(
                createPatientDto.FirstName,
                createPatientDto.LastName,
                createPatientDto.PhoneNumber,
                user.ApplicationUserId,
                username,
                email,
                createPatientDto.Diagnosis
            );
            patient.ClinicId = clinicId;
            patient.TherapistId = createPatientDto.TherapistId;

            _dbContext.Users.Add(user);
            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync();

            return new PatientDto
            {
                PatientId = patient.PatientId,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Username = patient.Username,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                Diagnosis = patient.Diagnosis,
                IsActive = patient.IsActive,
                TemporaryPassword = temporaryPassword
            };
        }

        public async Task<PatientDto?> UpdateAsync(Guid id, UpdatePatientDto updatePatientDto)
        {
            var patient = await _dbContext.Patients
                .SingleOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return null;

            var username = updatePatientDto.Username.Trim().ToLowerInvariant();
            var usernameInUse = await _dbContext.Users
                .AnyAsync(u => u.Username == username && u.ApplicationUserId != patient.ApplicationUserId);
            if (usernameInUse)
                throw new UsernameInUseException(username);

            var email = string.IsNullOrWhiteSpace(updatePatientDto.Email)
                ? null
                : updatePatientDto.Email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.ApplicationUserId == patient.ApplicationUserId);
            if (user != null)
            {
                user.Username = username;
                user.Email = email;
            }

            patient.FirstName = updatePatientDto.FirstName;
            patient.LastName = updatePatientDto.LastName;
            patient.Username = username;
            patient.Email = email;
            patient.PhoneNumber = updatePatientDto.PhoneNumber;
            patient.Diagnosis = updatePatientDto.Diagnosis;
            patient.IsActive = updatePatientDto.IsActive;
            patient.TherapistId = updatePatientDto.TherapistId;

            await _dbContext.SaveChangesAsync();

            return new PatientDto
            {
                PatientId = patient.PatientId,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Username = patient.Username,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                Diagnosis = patient.Diagnosis,
                TherapistId = patient.TherapistId,
                IsActive = patient.IsActive
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var patient = await _dbContext.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return false;

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.ApplicationUserId == patient.ApplicationUserId);

            patient.IsDeleted = true;
            if (user != null) user.IsDeleted = true;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private static string GeneratePassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            return RandomNumberGenerator.GetString(chars, length);
        }
    }
}
