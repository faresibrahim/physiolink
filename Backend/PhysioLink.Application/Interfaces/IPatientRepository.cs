using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.DTOs.Profile;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetPatientByUserIdAsync(Guid applicationUserId);

        public Task<PatientProfileDto> GetPatientProfileAsync (Guid patientId);
        public Task<bool> UpdatePatientProfileAsync(Guid patientId, UpdatePatientProfileDto request);

        public Task<PatientProgressDto> GetPatientProgressAsync(Guid patientId);

        public Task<bool> UpdateDeviceTokenAsync(Guid patientId, string deviceToken);

        public Task ClearDeviceTokenAsync(Guid applicationUserId);

        // Null only when the patient doesn't exist; a patient who never set a
        // reminder gets the column defaults (disabled, 18:00).
        public Task<ReminderDto?> GetReminderAsync(Guid patientId);

        // markHandledOn: when set, LastReminderSentOn is set to this clinic-local
        // date so today's (already past) reminder time doesn't fire late.
        public Task<bool> UpdateReminderAsync(Guid patientId, bool enabled, TimeOnly time, DateOnly? markHandledOn);
    }
}