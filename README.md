# PhysioLink — Physiotherapy Patient App

> A production-grade mobile application for physiotherapy clinics — built with Flutter & ASP.NET Core. Therapists assign exercises and appointments; patients track progress, submit RPE feedback, and manage their care — all in one place.

[![Flutter](https://img.shields.io/badge/Flutter-3.x-02569B?logo=flutter)](https://flutter.dev)
[![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org)
[![Railway](https://img.shields.io/badge/Deployed-Railway-0B0D0E?logo=railway)](https://railway.app)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---


## Key Features

| Feature | Detail |
|---|---|
| Exercise tracking | Sets, reps, duration, and difficulty per assigned exercise |
| RPE feedback | Animated Borg RPE 1–10 dialog; submitted per exercise completion |
| Appointments | Upcoming and past appointments with status badges |
| Profile | Patient profile view and management |
| JWT auth | Login with auto-refresh on token expiry; encrypted local storage |
| Responsive UI | 2-column grid on tablet (≥ 600 dp), list on phone |
| Shimmer loading | Skeleton states on every data screen |
| Typed error handling | Sealed `AppFailure` — `NetworkFailure / ServerFailure / AuthFailure` with retry actions |

---

## What's Live

| Resource | URL | Description |
|---|---|---|
| Patient App API | [`physiolink-production.up.railway.app`](https://physiolink-production.up.railway.app) | REST API consumed by the Flutter app |
| Admin Panel | [`physiolink-production-b9b6.up.railway.app`](https://physiolink-production-b9b6.up.railway.app) | ASP.NET Core MVC panel for clinic staff — dashboard, patient management, therapist management, exercise assignment, and appointment scheduling |

Swagger is disabled in production for security; available in local development at `/swagger`.

---

## Tech Stack

### Flutter — Patient App

| Layer | Package |
|---|---|
| State management | `flutter_riverpod` |
| Navigation | `go_router` |
| Networking | `dio` |
| Models | `freezed` + `json_serializable` |
| Auth storage | `flutter_secure_storage` |
| Functional error types | `dartz` (`Either`) |
| Loading UX | `shimmer` |
| Testing | `flutter_test` + `mocktail` |

### ASP.NET Core — Backend API

| Layer | Tool |
|---|---|
| Framework | ASP.NET Core (.NET 9) Web API |
| ORM | Entity Framework Core (Fluent API) |
| Database | PostgreSQL 16 |
| Auth | JWT Bearer + Refresh Tokens |
| Validation | FluentValidation |
| Error handling | ProblemDetails + global exception middleware |
| API docs | Swagger / OpenAPI |
| Deployment | Railway |

### ASP.NET Core — Admin Panel

| Layer | Tool |
|---|---|
| Framework | ASP.NET Core (.NET 9) MVC, Razor Views |
| Auth | Cookie authentication |
| API access | `IHttpClientFactory` — calls the Patient App API |
| Searchable selects | Tom Select |
| Deployment | Railway |

---

## Architecture

### Flutter — Feature-first Clean Architecture

```
lib/
├── core/
│   ├── network/          # DioClient, AuthInterceptor
│   ├── error/            # AppFailure sealed class
│   └── theme/            # AppColors, AppTextStyles, AppSpacing
├── features/
│   ├── auth/
│   │   ├── data/         # AuthRepositoryImpl, DTOs
│   │   ├── domain/       # AuthRepository (abstract)
│   │   └── presentation/ # LoginPage, AuthNotifier
│   ├── exercises/
│   │   ├── data/         # DioExerciseRepository, ExerciseDto
│   │   ├── domain/       # ExerciseRepository, ExerciseAssignment
│   │   └── presentation/ # ExercisesPage, ExerciseDetailPage, providers
│   ├── appointments/
│   ├── profile/
│   └── homepage/
└── main.dart
```

**Architecture decisions worth noting:**

- No `usecases/` layer — repositories are called directly from Riverpod providers, keeping the call chain short for an app of this scope
- `freezed` models serve as domain entities; no separate DTO layer duplication
- `_mapError()` in every Dio repository — single place mapping `DioException` → `AppFailure`
- `ConsumerStatefulWidget` for screens with local UI state; `ConsumerWidget` for provider-only screens
- Circular dependency between `AuthNotifier` and `DioClient` solved via token-getter lambda injected at construction time
- Separate `refreshDio` instance prevents infinite 401 retry loops during token refresh

### ASP.NET Core — 4-Layer Clean Architecture

```
API → Application → Domain → Infrastructure
```

---

## Getting Started

### Prerequisites

- Flutter SDK `>=3.x` / Dart `>=3.x`
- Android emulator or physical device
- The live API is already deployed — **no local backend setup required** to run the Flutter app

### Run the App

```bash
# 1. Clone
git clone https://github.com/faresibrahim/physiolink.git
cd physiolink/Frontend

# 2. Install dependencies
flutter pub get

# 3. Generate freezed + json_serializable code
dart run build_runner build --delete-conflicting-outputs

# 4. Run against the live API
flutter run --dart-define=BASE_URL=https://physiolink-production.up.railway.app
```

### Demo Credentials

| Surface | Email | Password |
|---|---|---|
| Flutter app (patient) | `john.smith@example.com` | `patient@123` |
| Admin panel (ClinicAdmin) | `fares.a.ibrahim@gmail.com` | `test@123` |

---

## Tests

```bash
flutter test
```

Coverage includes:

- `DioExerciseRepository` — JSON mapping, error routing, feedback submission
- `appointmentsProvider` — success state, `AuthFailure` guard
- `AuthNotifier` — login success (JWT decode + state population), login failure (rethrow + clean state)

---

## Backend — Local Setup

See [`Backend/README.md`](./Backend/README.md) for full instructions:

- .NET 9 + PostgreSQL local setup
- EF Core migrations
- Environment variable configuration
- Railway deployment

---

## Roadmap

- [ ] Video exercise library — MP4/H.264 via Cloudflare Stream
- [ ] Progress screen — completion rate and RPE trend charts
- [ ] Push notifications — appointment reminders
- [ ] iOS build

---

## Built With

`Flutter` · `Dart` · `Riverpod` · `go_router` · `dio` · `freezed` · `ASP.NET Core` · `.NET 9` · `Entity Framework Core` · `PostgreSQL` · `JWT` · `Railway`

---

## License

MIT © Fares Ibrahim
