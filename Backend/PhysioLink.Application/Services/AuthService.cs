using Microsoft.AspNetCore.Identity;
using PhysioLink.Application.DTOs.Auth;
using PhysioLink.Application.Interfaces;

using PhysioLink.Domain.Entities;

namespace PhysioLink.Application.Services
{
    public class AuthService : IAuthService
    {

        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ITokenService _tokenService;

        private readonly IUserRepository _userRepository;
        private readonly IPatientRepository _patientRepository;


        public AuthService(IPasswordHasher<ApplicationUser> passwordHasher, ITokenService tokenService, IUserRepository userRepository, IPatientRepository patientRepository)
        {
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _userRepository = userRepository;
            _patientRepository = patientRepository;
        }


        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            var userEmail = loginRequest.Email;
            var userPassword = loginRequest.Password;
            

            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            if(user == null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user,  user.PasswordHash, userPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // LoginAsync — after password verification passes:
            var patient = await _patientRepository.GetPatientByUserIdAsync(user.ApplicationUserId);
            var accessToken = _tokenService.GenerateAccessToken(user, patient?.PatientId);

            

            var refreshToken = _tokenService.GenerateRefreshToken();
            
            user.RefreshToken=refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            
        
            
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        }

        public async Task<bool> LogoutAsync(string token)
        {
            

            ApplicationUser? user = await _userRepository.GetUserByTokenAsync(token);
            if(user != null){
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            
            await _userRepository.UpdateAsync(user);
            return true;
            }

            return false;

        }

        public async Task<AuthResponseDto?> RefreshAsync(string token)
        {
            var user = await _userRepository.GetUserByTokenAsync(token);
            if(user == null) {return null;}
            
            if (user.RefreshTokenExpiry < DateTime.UtcNow)
            {
               return null;
            }

            // RefreshAsync — same:
            var patient = await _patientRepository.GetPatientByUserIdAsync(user.ApplicationUserId);
            var generatedAccessToken = _tokenService.GenerateAccessToken(user, patient?.PatientId);

            
            var generatedRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken=generatedRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);
            return new AuthResponseDto
            {
                AccessToken = generatedAccessToken,
                RefreshToken = generatedRefreshToken
            };
        }
    }
}