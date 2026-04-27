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
    }
}