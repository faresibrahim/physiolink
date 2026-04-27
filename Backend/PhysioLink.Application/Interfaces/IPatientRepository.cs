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
    }
}