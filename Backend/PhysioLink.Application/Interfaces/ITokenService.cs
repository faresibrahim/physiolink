
using PhysioLink.Domain.Entities;

namespace PhysioLink.Application.Interfaces
{
    //We've created 2 properties inside AuthResponseDto.cs, Refresh and Access tokens.
    //Inside this interface we need to generate those tokens.
    //No async calls because these are only calculations 
    public interface ITokenService
    {
        public string GenerateAccessToken(ApplicationUser user, Guid? patientId);
        public string GenerateRefreshToken();
    }
}