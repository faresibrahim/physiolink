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
        
    public string GenerateAccessToken(ApplicationUser user, Guid? patientId)
    {
    var claims = new List <Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, patientId?.ToString() ?? user.ApplicationUserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Name, user.FirstName + " " + user.LastName),

    };

    var secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
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