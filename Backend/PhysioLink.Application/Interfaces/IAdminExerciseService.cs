using PhysioLink.Application.DTOs.Exercises;

namespace PhysioLink.Application.Interfaces
{
    public interface IAdminExerciseService
    {
        Task<List<AdminExerciseDto>> GetAllAsync(string? search = null, string? difficulty = null);
        Task<AdminExerciseDto> CreateAsync(CreateAdminExerciseDto dto);
    }
}
