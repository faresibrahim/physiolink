using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PhysioLink.API.Extensions;
using PhysioLink.API.RateLimiting;
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

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
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

    // Re-authenticates the current caller before a destructive action (e.g. deactivating
    // a patient). Verification only — no tokens are issued or rotated, so the caller's
    // session survives untouched.
    // A wrong password returns 400, not 401: clients treat 401 as "access token expired"
    // and would otherwise refresh and retry a request that can never succeed.
    [Authorize]
    [HttpPost("verify-password")]
    public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequestDto request)
    {
        var email = User.GetEmail();

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var verified = await _authService.VerifyPasswordAsync(email, request.Password);
        if (!verified)
        {
            return BadRequest();
        }

        return NoContent();
    }


    // Lets the authenticated patient replace their temporary password with one they choose.
    // Called on first login while MustChangePassword is set, but also usable any time.
    // A wrong current password returns 400 (not 401) so clients don't treat it as an
    // expired access token and refresh-retry — same rationale as verify-password above.
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var email = User.GetEmail();

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(email, request.CurrentPassword, request.NewPassword);
        if (result == null)
        {
            return BadRequest();
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


