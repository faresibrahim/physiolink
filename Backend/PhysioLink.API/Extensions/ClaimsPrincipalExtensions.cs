using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PhysioLink.API.Extensions
{
    // Single home for reading identity out of the authenticated caller's JWT, so the
    // exact claim names and fallbacks live in one place instead of being copy-pasted
    // across controllers.
    public static class ClaimsPrincipalExtensions
    {
        // The JWT 'sub' is the ApplicationUserId. Default inbound mapping turns 'sub'
        // into NameIdentifier, so try that first with raw-claim fallbacks.
        public static Guid? GetApplicationUserId(this ClaimsPrincipal user)
        {
            var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        // Email is issued as the JWT 'email' claim; default inbound mapping may surface
        // it as ClaimTypes.Email, so check that first with a raw-claim fallback.
        public static string? GetEmail(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Email)?.Value
               ?? user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }
}
