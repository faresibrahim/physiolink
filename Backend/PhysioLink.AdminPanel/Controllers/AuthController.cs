using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels;
using System.Security.Claims;

namespace PhysioLink.AdminPanel.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly IWebHostEnvironment _environment;
        public AuthController(ApiClient apiClient, IWebHostEnvironment environment) {
            _apiClient = apiClient;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Only ever set when the last login checked "Remember me" — lets the
            // login page greet a returning admin by name and preselect the checkbox.
            var rememberedEmail = Request.Cookies["remember_email"];
            var model = new LoginViewModel
            {
                Email = rememberedEmail ?? string.Empty,
                Password = string.Empty,
                RememberMe = !string.IsNullOrEmpty(rememberedEmail)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            var loginRequest = new LoginRequest
            {
                Email = loginViewModel.Email,
                Password = loginViewModel.Password,
            };
            var response = await _apiClient.LoginAsync(loginRequest);
            if (response == null)
            {
                TempData["ErrorMessage"] = "Invalid email or password";
                return RedirectToAction("Login");
            }

            // The admin panel is for clinic admins only. A valid patient (or any other
            // role) authenticates against the shared API but has nothing to do here, so
            // treat it as an invalid login rather than signing them into a dead panel.
            if (!string.Equals(response.Role, "ClinicAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Invalid email or password";
                return RedirectToAction("Login");
            }

            // Tell ASP.NET Core auth middleware the user is authenticated.
            // Without this, [Authorize] on Dashboard never sees a signed-in user
            // and immediately redirects back to /Auth/Login regardless of the cookie.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, loginViewModel.Email),
                new Claim(ClaimTypes.Role, response.Role),
                new Claim("ClinicName", response.ClinicName ?? string.Empty)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Remember me extends the session to 30 days (capped in practice by the
            // API's 7-day refresh-token sliding expiry — this just keeps the browser
            // cookies alive long enough for that refresh to keep working). Without
            // IsPersistent the auth cookie is session-only and dies when the browser closes.
            var sessionLength = loginViewModel.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(8);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = loginViewModel.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(sessionLength)
                });

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(sessionLength)
            };

            Response.Cookies.Append("auth_token", response.AccessToken!, cookieOptions);
            Response.Cookies.Append("refresh_token", response.RefreshToken!, cookieOptions);

            if (loginViewModel.RememberMe)
            {
                Response.Cookies.Append("remember_email", loginViewModel.Email, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_environment.IsDevelopment(),
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }
            else
            {
                Response.Cookies.Delete("remember_email");
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _apiClient.LogoutAsync(refreshToken);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("auth_token");
            Response.Cookies.Delete("refresh_token");

            return RedirectToAction("Login");
        }
    }
}