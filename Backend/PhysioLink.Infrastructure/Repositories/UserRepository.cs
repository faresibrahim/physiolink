
using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PhysioLinkDbContext _dbContext;

        public UserRepository(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            
            return await _dbContext.Users.FirstOrDefaultAsync(u=> u.Email == email);

        }

        public async Task<ApplicationUser?> GetUserByTokenAsync(string token)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(t=>t.RefreshToken==token);
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string?> GetClinicNameAsync(Guid clinicId)
        {
            return await _dbContext.Clinics
                .IgnoreQueryFilters()
                .Where(c => c.ClinicId == clinicId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }
    }
}