
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.DTOs.Profile;

namespace PhysioLink.Application.Interfaces
{
    public interface IPatientService
    {
        Task<PatientProfileDto> GetPatientProfileAsync(Guid patientId);
        Task<bool> UpdatePatientProfileAsync(Guid patientId, UpdatePatientProfileDto request);
        Task<PatientProgressDto> GetPatientProgressAsync(Guid patientId);

        // Resolves the caller's own PatientId from their authenticated ApplicationUserId
        // (JWT 'sub'). Returns null if the user has no patient record. Used to enforce
        // that a patient can only act on their own data.
        Task<Guid?> ResolvePatientIdAsync(Guid applicationUserId);
    }
}