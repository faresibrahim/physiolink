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
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");


app.Run();
