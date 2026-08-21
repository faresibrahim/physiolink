using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Services {
    public class TokenService : ITokenService
    {
        private readonly string _jwt;
        public TokenService() {
           _jwt = Environment.GetEnvironmentVariable("JWT_SECRET")!;
        }
        
    public string GenerateAccessToken(ApplicationUser user)
    {
    var claims = new List <Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.ApplicationUserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Name, user.FirstName + " " + user.LastName),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("ClinicId", user.ClinicId.ToString()),
    };

   
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "physiolink-api",
        audience: "physiolink-flutter",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

        public string GenerateRefreshToken()
        {
            byte[] bytes= new byte[32];
            RandomNumberGenerator.Fill(bytes);
	        var result = Convert.ToBase64String(bytes);
            return result;
        }
    }
}