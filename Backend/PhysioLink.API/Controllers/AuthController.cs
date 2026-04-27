using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Auth;
using PhysioLink.Application.Interfaces;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
    {
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        AuthResponseDto? result = await _authService.LoginAsync(request);
        if(result == null)
        {
            return Unauthorized();
        }
        
        
            return Ok(result);
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> GenerateRefreshToken([FromBody]RefreshRequestDto request)
    {
        var refreshToken = request.RefreshToken;
        var result = await _authService.RefreshAsync(refreshToken);
        if(result == null)
        {
            return Unauthorized();
        }
        return Ok(result);

    }

    [HttpPost("logout")]
    public async Task<IActionResult> UserLogoutAsync([FromBody] RefreshRequestDto refreshToken)
    {
       var isLoggedOut = await _authService.LogoutAsync(refreshToken.RefreshToken);
       if(isLoggedOut == true)
        {
            return NoContent();
        }
        return Unauthorized();
    }

    }


