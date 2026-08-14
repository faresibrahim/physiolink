using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhysioLink.API.Middleware;
using PhysioLink.API.RateLimiting;
using PhysioLink.Application.Interfaces;
using PhysioLink.Application.Services;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;
using PhysioLink.Infrastructure.Repositories;
using FluentValidation.AspNetCore;
using PhysioLink.Infrastructure.Services;
using FluentValidation;
using PhysioLink.Application.Validators.Appointments;
using Microsoft.OpenApi.Models;



//Phase 1 build services(This is where the tools I need)
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//add swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAuthorization();
// Shared secret that lets the trusted first-party admin panel bypass IP rate limiting.
// Dev falls back to a well-known value so both apps match out of the box; production
// MUST set INTERNAL_API_KEY (same value on the API and the admin panel).
var internalApiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY")
    ?? (builder.Environment.IsDevelopment() ? "dev-internal-key" : null);
builder.Services.AddApiRateLimiting(internalApiKey);
builder.Services.AddMemoryCache();



builder.Services.AddDbContext<PhysioLinkDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));



builder.Services.AddValidatorsFromAssemblyContaining<AppointmentValidator>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); //this line is required because it tells ASP.Net Core to use the ProblemDetails
                                      //format throughout the pipeline, not only in the handler.
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExerciseRepository,ExerciseRepository>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IAppointmentRepository,AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPatientRepository,PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ICurrentClinicService, CurrentClinicService>();
builder.Services.AddScoped<IAdminTherapistService, AdminTherapistService>();
builder.Services.AddScoped<IAdminPatientService, AdminPatientService>();
builder.Services.AddScoped<IAdminAssignmentService, AdminAssignmentService>();
builder.Services.AddScoped<IAdminAppointmentService, AdminAppointmentService>();
builder.Services.AddScoped<IAdminSlotService, AdminSlotService>();
builder.Services.AddScoped<ISlotExpiryService, SlotExpiryService>();
builder.Services.AddScoped<IPatientSlotService, PatientSlotService>();
builder.Services.AddScoped<IAdminExerciseService, AdminExerciseService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "physiolink-api",
            ValidAudience = "physiolink-flutter",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!))
        };
    });

//Phase 2 configure http request pipelines(this is what happens when a request comes in)
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhysioLinkDbContext>();
    await db.Database.MigrateAsync();
}

await DbSeeder.SeedAsync(app.Services);



if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    // Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

// Serve wwwroot/ (e.g. /images/*.mov demo videos) so the mobile app can stream
// exercise videos over the network. Placed before auth so files stay public.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Rate limiting sits after auth so limiter partitions could later key off the
// authenticated user, and before endpoints so throttled requests never reach them.
app.UseRateLimiter();

app.MapControllers();
app.Run();

