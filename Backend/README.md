# PhysioLink — ASP.NET Core API

> REST API backend for the PhysioLink physiotherapy patient app. Built with ASP.NET Core (.NET 9), Entity Framework Core, and PostgreSQL. Deployed on Railway.

---

## Tech Stack

| Purpose | Tool |
|---|---|
| Framework | ASP.NET Core (.NET 9) Web API |
| ORM | Entity Framework Core (Fluent API) |
| Database | PostgreSQL |
| Auth | JWT Bearer + Refresh Token rotation |
| Validation | FluentValidation |
| Error handling | ProblemDetails + global exception middleware |
| API docs | Swashbuckle (Swagger UI) |
| Deployment | Railway |

---

## Solution Structure

```
PhysioLink/
├── PhysioLink.API/             # Controllers, middleware, Program.cs
│   ├── Controllers/            # Thin — validate → service → return
│   └── Middleware/             # Global exception handler
├── PhysioLink.Application/     # Business logic
│   ├── Interfaces/             # IAuthService, IExerciseService, etc.
│   ├── Services/               # Service implementations
│   ├── DTOs/                   # Request + Response DTOs
│   └── Validators/             # FluentValidation validators
├── PhysioLink.Domain/          # Core domain
│   ├── Entities/               # ApplicationUser, Patient, Exercise, etc.
│   └── Enums/                  # DifficultyLevel, AppointmentStatus, etc.
├── PhysioLink.Infrastructure/  # Data access
│   ├── Data/                   # PhysioLinkDbContext, EntityConfigurations/
│   ├── Repositories/           # EF Core implementations
│   └── Migrations/             # EF Core migrations
└── PhysioLink.Tests/
    ├── Unit/                   # Service + repository tests
    └── Integration/            # WebApplicationFactory API tests
```

**Key architecture decisions:**

- Fluent API only — zero data annotations on domain entities
- GUID primary keys on all entities
- `AuditableEntity` base class — `CreatedAt`, `UpdatedAt`, `IsDeleted` on every entity
- DTOs in every response — EF entities never exposed directly
- JWT secret from environment variables only — never `appsettings.json`

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/v1/auth/login` | Login → returns `AccessToken` + `RefreshToken` |
| POST | `/api/v1/auth/refresh` | Rotate refresh token |
| POST | `/api/v1/auth/logout` | Invalidate refresh token |

### Patients

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/patients/{id}/exercises` | Assigned exercises |
| GET | `/api/v1/exercises/{id}` | Single exercise detail |
| POST | `/api/v1/exercises/{id}/feedback` | Submit RPE feedback (1–10) |
| GET | `/api/v1/patients/{id}/appointments` | Patient appointments |
| POST | `/api/v1/appointments` | Request new appointment |
| GET | `/api/v1/patients/{id}/profile` | Patient profile |
| PUT | `/api/v1/patients/{id}/profile` | Update profile |

Full interactive documentation at `/swagger`.

---

## Local Setup

### Prerequisites

- .NET 9 SDK
- PostgreSQL (local instance or Docker)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Environment Variables

Set the following in your environment or a local `.env` file. **Never commit secrets to source control.**

| Variable | Description |
|---|---|
| `JWT_SECRET` | JWT signing key — minimum 32 characters |
| `CONNECTION_STRING` | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |

### Run Locally

```bash
# 1. Navigate to the backend folder
cd Backend

# 2. Restore dependencies
dotnet restore

# 3. Set environment variables (PowerShell)
$env:JWT_SECRET="your-secret-key-min-32-chars"
$env:CONNECTION_STRING="Host=localhost;Port=5432;Database=physiolink;Username=postgres;Password=yourpassword"
$env:ASPNETCORE_ENVIRONMENT="Development"

# 4. Apply migrations
dotnet ef database update --project PhysioLink.Infrastructure --startup-project PhysioLink.API

# 5. Run the API
dotnet run --project PhysioLink.API
```

API: `http://localhost:5218`  
Swagger UI: `http://localhost:5218/swagger`

### Seed Data

Migrations include seed data:

- 1 therapist account
- 3 patients (including the test patient below)
- 5 exercises across varying difficulty levels

**Test patient credentials:**

| Field | Value |
|---|---|
| Email | `john.smith@example.com` |
| Password | `patient@123` |

---

## Deployment

Deployed on **Railway** with a managed PostgreSQL instance.

### Live URLs

| Resource | URL |
|---|---|
| API base | `https://physiolink-production.up.railway.app` |
| Swagger UI | `https://physiolink-production.up.railway.app/swagger` |

### Deploy Your Own

1. Create a project on [Railway](https://railway.app) and add a PostgreSQL plugin
2. Connect your GitHub repo — set root directory to `/Backend`
3. Set environment variables in the Railway dashboard:
   - `JWT_SECRET`
   - `CONNECTION_STRING` (Railway injects this automatically for the PostgreSQL plugin)
   - `ASPNETCORE_ENVIRONMENT` = `Production`
4. Railway builds via `Dockerfile` and deploys on every push to `main`
5. Run migrations post-deploy via the Railway dashboard shell:

```bash
dotnet ef database update --project PhysioLink.Infrastructure --startup-project PhysioLink.API
```

---

## Running Tests

```bash
cd Backend
dotnet test
```

---

## License

MIT
