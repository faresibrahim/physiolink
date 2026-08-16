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

Full interactive documentation at `/swagger` (local development only — disabled in production).

### Admin — Therapists

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/therapists` | List therapists |
| GET | `/api/v1/admin/therapists/{id}` | Therapist detail |
| POST | `/api/v1/admin/therapists` | Create therapist |
| PUT | `/api/v1/admin/therapists/{id}` | Update therapist |
| DELETE | `/api/v1/admin/therapists/{id}` | Delete therapist |

### Admin — Patients

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/patients` | List patients |
| GET | `/api/v1/admin/patients/{id}` | Patient detail |
| POST | `/api/v1/admin/patients` | Enroll patient |
| PUT | `/api/v1/admin/patients/{id}` | Update patient |
| DELETE | `/api/v1/admin/patients/{id}` | Delete patient |

### Admin — Appointments

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/appointments` | List appointments (paged, filterable by date/status) |
| GET | `/api/v1/admin/appointments/history` | Archive of past-due appointments (Completed/Rejected/Expired/Cancelled) |
| GET | `/api/v1/admin/appointments/{id}` | Appointment detail |
| POST | `/api/v1/admin/appointments` | Create appointment |
| PUT | `/api/v1/admin/appointments/{id}` | Update appointment |
| DELETE | `/api/v1/admin/appointments/{id}` | Delete appointment |
| GET | `/api/v1/admin/appointments/requests` | Pending appointment requests, optionally by therapist |
| PUT | `/api/v1/admin/appointments/{id}/accept` | Accept a request |
| PUT | `/api/v1/admin/appointments/{id}/reject` | Reject a request |
| PUT | `/api/v1/admin/appointments/{id}/cancel` | Cancel an appointment |

### Admin — Exercises

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/exercises` | List exercises (search, difficulty, category filters) |
| GET | `/api/v1/admin/exercises/{id}` | Exercise detail |
| POST | `/api/v1/admin/exercises` | Create exercise |

### Admin — Assignments

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/patients/{patientId}/assignments` | List a patient's assigned exercises |
| GET | `/api/v1/admin/assignments/{id}` | Assignment detail |
| POST | `/api/v1/admin/patients/{patientId}/assignments` | Assign an exercise to a patient |
| PUT | `/api/v1/admin/assignments/{id}` | Update assignment |
| DELETE | `/api/v1/admin/assignments/{id}` | Delete assignment |

### Admin — Slots

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/therapists/{therapistId}/slots` | Weekly availability grid |
| POST | `/api/v1/admin/therapists/{therapistId}/slots` | Open a slot |
| DELETE | `/api/v1/admin/therapists/{therapistId}/slots` | Close a slot |

### Admin — Dashboard

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/dashboard` | Clinic overview stats |

All `admin/*` routes require the `ClinicAdmin` role and are consumed by the [Admin Panel](../README.md#whats-live), not the Flutter app.

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

`DbSeeder` runs automatically on startup in `Development` (never in production):

- 2 clinics
- 6 therapists
- 10 patients (including the test patient below)
- 3 exercises across varying difficulty levels

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

Swagger UI is disabled in production for security; it's only available at `/swagger` when running locally in `Development`.

### Config-as-code

Two Railway services build from this repo, each pinned to its own `.toml`:

| File | Service | Dockerfile |
|---|---|---|
| `Backend/railway.toml` | API | `Backend/Dockerfile` |
| `Backend/railway.adminpanel.toml` | Admin Panel | `Backend/PhysioLink.AdminPanel/Dockerfile` |

Both configs build via Docker and expose a `/health` healthcheck.

### Deploy Your Own

1. Create a project on [Railway](https://railway.app) and add a PostgreSQL plugin
2. Connect your GitHub repo — set root directory to `/Backend`
3. Set environment variables in the Railway dashboard:
   - `JWT_SECRET`
   - `CONNECTION_STRING` (Railway injects this automatically for the PostgreSQL plugin)
   - `ASPNETCORE_ENVIRONMENT` = `Production`
4. Railway builds via `Dockerfile` and deploys on every push to `master`
5. Migrations run automatically — `Program.cs` calls `dbContext.Database.MigrateAsync()` on startup, before the app accepts requests. No manual migration step is needed post-deploy.

---

## Running Tests

```bash
cd Backend
dotnet test
```

---

## License

MIT
