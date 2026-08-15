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
            return View();
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

            // Tell ASP.NET Core auth middleware the user is authenticated.
            // Without this, [Authorize] on Dashboard never sees a signed-in user
            // and immediately redirects back to /Auth/Login regardless of the cookie.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, loginViewModel.Email),
                new Claim("ClinicName", response.ClinicName ?? string.Empty)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            };

            Response.Cookies.Append("auth_token", response.AccessToken!, cookieOptions);
            Response.Cookies.Append("refresh_token", response.RefreshToken!, cookieOptions);

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