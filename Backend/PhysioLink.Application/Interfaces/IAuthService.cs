using PhysioLink.Application.DTOs.Auth;

//this service class only handles business logic, find user, verify password, call token 
//and return response.
//We need Token Service class in order to validate the JWT Tokens
namespace PhysioLink.Application.Interfaces
{
    public interface IAuthService
    {
        public Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest);
        public Task<AuthResponseDto?> RefreshAsync(string token);

        public Task<bool> LogoutAsync(string token);

        //Replaces the temporary password with one the patient chooses on first login.
        //Verifies the current password, sets the new one, clears the must-change flag,
        //and rotates tokens so the returned session reflects the updated state.
        public Task<AuthResponseDto?> ChangePasswordAsync(Guid userId, string? currentPassword, string newPassword);

        //Confirms a password belongs to the given account without issuing or rotating
        //any tokens — used to re-authenticate before a destructive action.
        public Task<bool> VerifyPasswordAsync(Guid userId, string password);
    }
}