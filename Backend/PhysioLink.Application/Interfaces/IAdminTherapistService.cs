using PhysioLink.Application.DTOs.Therapists;
using PhysioLink.Application.DTOs;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminTherapistService
    {
        Task<PagedResult<TherapistDto>> GetAllAsync(int page, int pageSize);
        Task<TherapistDto?> GetByIdAsync(Guid id);
        Task<TherapistDto> CreateAsync(CreateTherapistDto createTherapistDto);
        Task<TherapistDto?> UpdateAsync(Guid id, UpdateTherapistDto updateTherapistDto);
        Task<bool> DeleteAsync(Guid id);
 
    }
}