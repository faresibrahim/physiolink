
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
    }
}