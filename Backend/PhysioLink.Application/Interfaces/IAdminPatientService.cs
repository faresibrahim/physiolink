using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.DTOs;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminPatientService
    {
        Task<PagedResult<PatientDto>> GetAllAsync(int page, int pageSize, Guid? therapistId, string? search = null);
        Task<PatientDetailDto?> GetByIdAsync(Guid id);
        Task<PatientDto> CreateAsync(CreatePatientDto createPatientDto);
        Task<PatientDto?> UpdateAsync(Guid id, UpdatePatientDto updatePatientDto);
        Task<bool> DeleteAsync(Guid id);
    }
}