
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
            _dbContext.Users.Update(user); //what does this line do?
            await _dbContext.SaveChangesAsync();
        }   
    }
}