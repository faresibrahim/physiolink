using Microsoft.AspNetCore.Authentication.Cookies;
using PhysioLink.AdminPanel.Filters;
using PhysioLink.AdminPanel.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Turns a dead session (ApiClient throws SessionExpiredException) into a clean
    // redirect to the login page instead of a silently-empty view full of zeros.
    options.Filters.Add<SessionExpiredExceptionFilter>();
}).AddRazorRuntimeCompilation();

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        // Force Secure outside Development so the session cookie is never sent over
        // plain HTTP — matching the auth_token/refresh_token cookies. Users reach
        // Railway over HTTPS at the edge, so the browser always has a secure channel.
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

builder.Services.AddHttpClient("PhysioLinkApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);

    // Identify this trusted first-party server to the API so its requests bypass the
    // API's per-IP rate limiting. Without this, every admin's login/traffic shares
    // this one server's IP and gets throttled collectively. Must match the API's
    // INTERNAL_API_KEY; dev falls back to the same well-known value on both sides.
    var internalApiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY")
        ?? (builder.Environment.IsDevelopment() ? "dev-internal-key" : null);
    if (!string.IsNullOrEmpty(internalApiKey))
    {
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", internalApiKey);
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Railway terminates TLS at its edge; the container only ever sees plain HTTP on
// :8080. Redirecting to HTTPS in-container does nothing useful in production and can
// turn Railway's HTTP healthcheck into a 307. Guard to Development, matching the API.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


// Liveness probe for Railway's healthcheck. Public + no I/O: it must return 200
// instantly even if the API is unreachable, so it reflects "is this process alive",
// not "is the backend API reachable". Kept off the default route (which is [Authorize]
// on DashboardController and 302-redirects to /Auth/Login when unauthenticated).
app.MapGet("/health", () => Results.Ok("healthy")).AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");


app.Run();
