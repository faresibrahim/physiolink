using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Assignments;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminAssignmentService
    {
        Task<PagedResult<AssignmentDto>> GetAllByPatientAsync(Guid patientId, int page, int pageSize);
        Task<AssignmentDto?> GetByIdAsync(Guid id);
        Task<AssignmentDto> CreateAsync(Guid patientId, CreateAssignmentDto dto);
        Task<AssignmentDto?> UpdateAsync(Guid id, UpdateAssignmentDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
