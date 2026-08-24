using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Assignments;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class AdminAssignmentService : IAdminAssignmentService
    {
        private readonly PhysioLinkDbContext _dbContext;
        private readonly IPushNotificationSender _pushSender;
        private readonly ILogger<AdminAssignmentService> _logger;

        public AdminAssignmentService(
            PhysioLinkDbContext dbContext,
            IPushNotificationSender pushSender,
            ILogger<AdminAssignmentService> logger)
        {
            _dbContext = dbContext;
            _pushSender = pushSender;
            _logger = logger;
        }

        public async Task<PagedResult<AssignmentDto>> GetAllByPatientAsync(Guid patientId, int page, int pageSize)
        {
            var query = _dbContext.ExerciseAssignments.AsNoTracking()
                .Where(ea => ea.PatientId == patientId)
                .Select(ea => new AssignmentDto
                {
                    ExerciseAssignmentId = ea.ExerciseAssignmentId,
                    PatientId = ea.PatientId,
                    ExerciseId = ea.ExerciseId,
                    ExerciseName = ea.Exercise.Name,
                    TherapistName = ea.TherapistName,
                    Sets = ea.Sets,
                    Reps = ea.Reps,
                    DurationMinutes = ea.DurationMinutes,
                    FrequencyPerWeek = ea.FrequencyPerWeek,
                    FrequencyPerDay = ea.FrequencyPerDay,
                    Status = ea.Status,
                    Feedback = ea.Feedback,
                    AssignedAt = ea.AssignedAt,
                    CompletedAt = ea.CompletedAt
                });

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AssignmentDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<AssignmentDto?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ExerciseAssignments.AsNoTracking()
                .Where(ea => ea.ExerciseAssignmentId == id)
                .Select(ea => new AssignmentDto
                {
                    ExerciseAssignmentId = ea.ExerciseAssignmentId,
                    PatientId = ea.PatientId,
                    ExerciseId = ea.ExerciseId,
                    ExerciseName = ea.Exercise.Name,
                    TherapistName = ea.TherapistName,
                    Sets = ea.Sets,
                    Reps = ea.Reps,
                    DurationMinutes = ea.DurationMinutes,
                    FrequencyPerWeek = ea.FrequencyPerWeek,
                    FrequencyPerDay = ea.FrequencyPerDay,
                    Status = ea.Status,
                    Feedback = ea.Feedback,
                    AssignedAt = ea.AssignedAt,
                    CompletedAt = ea.CompletedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AssignmentDto> CreateAsync(Guid patientId, CreateAssignmentDto dto)
        {
            var exerciseName = await _dbContext.Exercises.AsNoTracking()
                .Where(e => e.ExerciseId == dto.ExerciseId)
                .Select(e => e.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            var assignment = new ExerciseAssignment(
                dto.TherapistName,
                patientId,
                dto.ExerciseId,
                dto.Sets,
                dto.Reps,
                dto.DurationMinutes,
                dto.FrequencyPerWeek,
                dto.FrequencyPerDay,
                assignedAt: DateTime.UtcNow
            );

            _dbContext.ExerciseAssignments.Add(assignment);
            await _dbContext.SaveChangesAsync();

            await SendAssignmentPushAsync(patientId, assignment, exerciseName);

            return new AssignmentDto
            {
                ExerciseAssignmentId = assignment.ExerciseAssignmentId,
                PatientId = assignment.PatientId,
                ExerciseId = assignment.ExerciseId,
                ExerciseName = exerciseName,
                TherapistName = assignment.TherapistName,
                Sets = assignment.Sets,
                Reps = assignment.Reps,
                DurationMinutes = assignment.DurationMinutes,
                FrequencyPerWeek = assignment.FrequencyPerWeek,
                FrequencyPerDay = assignment.FrequencyPerDay,
                Status = assignment.Status,
                Feedback = assignment.Feedback,
                AssignedAt = assignment.AssignedAt,
                CompletedAt = assignment.CompletedAt
            };
        }

        // Best-effort — a dead token or FCM outage must never fail the assignment
        // itself, which already committed above.
        private async Task SendAssignmentPushAsync(Guid patientId, ExerciseAssignment assignment, string exerciseName)
        {
            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (string.IsNullOrWhiteSpace(patient?.DeviceToken)) return;

            try
            {
                await _pushSender.SendAsync(
                    patient.DeviceToken,
                    "New exercise assigned",
                    exerciseName,
                    new Dictionary<string, string>
                    {
                        ["type"] = "exercise",
                        ["id"] = assignment.ExerciseAssignmentId.ToString(),
                    });
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                patient.DeviceToken = null;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push notification failed for patient {PatientId}", patientId);
            }
        }

        public async Task<AssignmentDto?> UpdateAsync(Guid id, UpdateAssignmentDto dto)
        {
            var assignment = await _dbContext.ExerciseAssignments
                .SingleOrDefaultAsync(ea => ea.ExerciseAssignmentId == id);

            if (assignment == null) return null;

            var exerciseName = await _dbContext.Exercises.AsNoTracking()
                .Where(e => e.ExerciseId == assignment.ExerciseId)
                .Select(e => e.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            assignment.TherapistName = dto.TherapistName;
            assignment.Sets = dto.Sets;
            assignment.Reps = dto.Reps;
            assignment.DurationMinutes = dto.DurationMinutes;
            assignment.FrequencyPerWeek = dto.FrequencyPerWeek;
            assignment.FrequencyPerDay = dto.FrequencyPerDay;
            assignment.Status = dto.Status;

            await _dbContext.SaveChangesAsync();

            return new AssignmentDto
            {
                ExerciseAssignmentId = assignment.ExerciseAssignmentId,
                PatientId = assignment.PatientId,
                ExerciseId = assignment.ExerciseId,
                ExerciseName = exerciseName,
                TherapistName = assignment.TherapistName,
                Sets = assignment.Sets,
                Reps = assignment.Reps,
                DurationMinutes = assignment.DurationMinutes,
                FrequencyPerWeek = assignment.FrequencyPerWeek,
                FrequencyPerDay = assignment.FrequencyPerDay,
                Status = assignment.Status,
                Feedback = assignment.Feedback,
                AssignedAt = assignment.AssignedAt,
                CompletedAt = assignment.CompletedAt
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var assignment = await _dbContext.ExerciseAssignments
                .FirstOrDefaultAsync(ea => ea.ExerciseAssignmentId == id);

            if (assignment == null) return false;

            assignment.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
