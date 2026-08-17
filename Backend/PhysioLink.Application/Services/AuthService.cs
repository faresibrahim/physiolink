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

            // LoginAsync � after password verification passes:
            var (accessToken, refreshToken) = await IssueTokensAsync(user);

            var patient = await _patientRepository.GetPatientByUserIdAsync(user.ApplicationUserId);
            var clinicName = await _userRepository.GetClinicNameAsync(user.ClinicId);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                PatientId = patient?.PatientId ?? Guid.Empty,
                ClinicName = clinicName,
                Role = user.Role,
                MustChangePassword = user.MustChangePassword
            };

        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result != PasswordVerificationResult.Failed;
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

            // RefreshAsync � same:
            var (accessToken, refreshToken) = await IssueTokensAsync(user);

            var patient = await _patientRepository.GetPatientByUserIdAsync(user.ApplicationUserId);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                PatientId = patient?.PatientId ?? Guid.Empty,
                MustChangePassword = user.MustChangePassword
            };
        }

        public async Task<AuthResponseDto?> ChangePasswordAsync(string email, string? currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
            {
                return null;
            }

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            // First-time forced change: the patient already proved they hold the
            // temporary password by logging in with it, so we don't ask for it again.
            // A voluntary change (must-change flag already cleared) still requires the
            // current password to be supplied and verified.
            if (!user.MustChangePassword)
            {
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    return null;
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
                if (result == PasswordVerificationResult.Failed)
                {
                    return null;
                }
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.MustChangePassword = false;

            // Rotate tokens so the caller ends up with a fresh session that reflects
            // the cleared must-change flag, rather than reusing the temporary-password one.
            // IssueTokensAsync's single UpdateAsync persists the password change too.
            var (accessToken, refreshToken) = await IssueTokensAsync(user);

            var patient = await _patientRepository.GetPatientByUserIdAsync(user.ApplicationUserId);
            var clinicName = await _userRepository.GetClinicNameAsync(user.ClinicId);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                PatientId = patient?.PatientId ?? Guid.Empty,
                ClinicName = clinicName,
                MustChangePassword = false
            };
        }

        // Issues a fresh access token and rotates the refresh token onto the user
        // (7-day sliding expiry), persisting the change with a single UpdateAsync.
        // Any other pending edits already made to `user` are saved in the same call.
        private async Task<(string AccessToken, string RefreshToken)> IssueTokensAsync(ApplicationUser user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);
            return (accessToken, refreshToken);
        }
    }
}