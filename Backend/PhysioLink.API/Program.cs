using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhysioLink.API.Middleware;
using PhysioLink.Application.Interfaces;
using PhysioLink.Application.Services;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;
using PhysioLink.Infrastructure.Repositories;
using FluentValidation.AspNetCore;
using PhysioLink.Infrastructure.Services;
using FluentValidation;
using PhysioLink.Application.Validators.Appointments;


//Phase 1 build services(This is where the tools I need)
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//add swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAuthorization();



builder.Services.AddDbContext<PhysioLinkDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));



builder.Services.AddValidatorsFromAssemblyContaining<AppointmentValidator>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); //this line is required because it tells ASP.Net Core to use the ProblemDetails
                                      //format throughout the pipeline, not only in the handler.

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

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

