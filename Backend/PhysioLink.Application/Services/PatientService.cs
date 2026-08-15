using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.DTOs.Profile;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Application.Services
{
    public class PatientService : IPatientService
    {

        private readonly IPatientRepository _patientRepository;
        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientProfileDto> GetPatientProfileAsync(Guid patientId)
        {
            return await _patientRepository.GetPatientProfileAsync(patientId);
        }

        public async Task<PatientProgressDto> GetPatientProgressAsync(Guid patientId)
        {
            return await _patientRepository.GetPatientProgressAsync(patientId);
        }

        public async Task<bool> UpdatePatientProfileAsync(Guid patientId, UpdatePatientProfileDto request)
        {
    return await _patientRepository.UpdatePatientProfileAsync(patientId, request);
        }

        public async Task<Guid?> ResolvePatientIdAsync(Guid applicationUserId)
        {
            var patient = await _patientRepository.GetPatientByUserIdAsync(applicationUserId);
            return patient?.PatientId;
        }

       
    }
}