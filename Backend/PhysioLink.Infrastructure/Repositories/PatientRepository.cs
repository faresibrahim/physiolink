using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.DTOs.Profile;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        
        private readonly PhysioLinkDbContext _dbContext;
        public PatientRepository(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Patient?> GetPatientByUserIdAsync(Guid applicationUserId)
        {
            // Bypass the ClinicScoped filter — during login the JWT hasn't been
            // issued yet, so CurrentClinicService.GetCurrentClinicId() returns null
            // and the filter would exclude every patient.
            return await _dbContext.Patients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ApplicationUserId == applicationUserId);
        }


        public async Task<PatientProfileDto> GetPatientProfileAsync(Guid patientId)
        {
            // PatientId is unique — bypass clinic filter so the endpoint works
            // regardless of JWT-claim state.
            var patient = await _dbContext.Patients
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(p => p.Therapist)
                .Include(p => p.Clinic)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);
            if(patient is null)
                return null!;

            return new PatientProfileDto
            {
                PatientId = patient.PatientId,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                Diagnosis = patient.Diagnosis,
                PhoneNumber = patient.PhoneNumber,
                TherapistName = patient.Therapist != null
                    ? $"{patient.Therapist.FirstName} {patient.Therapist.LastName}"
                    : null,
                ClinicName = patient.Clinic?.Name,
            };
        }

        public async Task<bool> UpdatePatientProfileAsync(Guid patientId, UpdatePatientProfileDto request)
{
    var patient = await _dbContext.Patients
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.PatientId == patientId);

    if (patient is null)
        return false;

    patient.FirstName = request.FirstName;
    patient.LastName = request.LastName;
    patient.PhoneNumber = request.PhoneNumber;
    patient.Diagnosis = request.Diagnosis;

    await _dbContext.SaveChangesAsync();
    return true;
}



    public async Task<PatientProgressDto> GetPatientProgressAsync(Guid patientId)
    {
        var totalAppointments = await _dbContext.Appointments
            .IgnoreQueryFilters()
            .Where(p => p.PatientId == patientId)
            .CountAsync();

        var feedbackExercises = await _dbContext.ExerciseAssignments.IgnoreQueryFilters().Where(p=>p.PatientId == patientId && p.Feedback != default).CountAsync();

        var totalAssignedExercises = await _dbContext.ExerciseAssignments
            .IgnoreQueryFilters()
            .Where(p=>p.PatientId == patientId)
            .CountAsync();

        var lastRecentAppointment = await _dbContext.Appointments.IgnoreQueryFilters().Where(p=>p.PatientId == patientId).MaxAsync(p=> (DateTime?)p.AppointmentTime);

        return new PatientProgressDto
        {
            TotalAppointments = totalAppointments,
            ExercisesWithFeedback = feedbackExercises,
            TotalAssignedExercises = totalAssignedExercises,
            LastAppointmentDate = lastRecentAppointment
        };

    }
    }
}