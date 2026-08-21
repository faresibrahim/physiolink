using PhysioLink.Application.DTOs;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Application.Interfaces
{
    public interface IUserRepository
    {
        public Task<ApplicationUser?> GetUserByEmailAsync(string email);
        public Task<ApplicationUser?> GetUserByUsernameAsync(string username);
        public Task<ApplicationUser?> GetUserByIdAsync(Guid id);
        public Task<ApplicationUser?> GetUserByTokenAsync(string token);
        public Task UpdateAsync(ApplicationUser user);
        public Task<string?> GetClinicNameAsync(Guid clinicId);
    }
}