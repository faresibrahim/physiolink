# PhysioLink — Flutter Patient App

> The patient-facing mobile app. For the full tech stack, feature-first folder structure, and architecture decisions (no `usecases/` layer, `freezed` domain entities, the `AuthNotifier`/`DioClient` circular-dependency fix, etc.), see the [root README](../README.md) — this file covers what's specific to running, testing, and releasing this Flutter project day to day.

---

## Tech Stack

See the [root README's Tech Stack table](../README.md#tech-stack) — `flutter_riverpod`, `go_router`, `dio`, `freezed`/`json_serializable`, `flutter_secure_storage`, `dartz`, `shimmer`, `flutter_test`/`mocktail`. Nothing Flutter-specific to add beyond what's already documented there.

---

## Architecture Notes

Not fully covered in the root README:

- **Appointments is split into two repositories**, not one: `AppointmentsRepository` (`getMyAppointments` — the patient's own appointment history, including pending/rejected/expired/cancelled) and `SlotRepository` (`getMySlots` + `requestSlot` — browsing a therapist's open slots and requesting one). A `409` from `requestSlot` maps to a dedicated `SlotUnavailableFailure` so the UI can say "that slot was just taken" instead of a generic server error.
- API responses aren't always shaped like the Dart domain models — `DioAppointmentRepository._normalize()` reshapes the API's `PatientAppointmentDto` (`scheduledAt`, `type`) into the fields the `freezed` `Appointment` model expects (`appointmentTime`, `title`) before deserializing.

---

## Local Setup

### Prerequisites

- Flutter SDK `>=3.x` / Dart `>=3.x`
- Android emulator or physical device (see [Known Limitations](#known-limitations--deferred) — iOS isn't buildable yet)

### Run

```bash
flutter pub get
dart run build_runner build --delete-conflicting-outputs
flutter run --dart-define=BASE_URL=https://physiolink-production.up.railway.app
```

`BASE_URL` is read in [`lib/core/network/api_config.dart`](lib/core/network/api_config.dart) via `String.fromEnvironment`. If omitted, it defaults to `http://10.0.2.2:5218` — the Android emulator's alias for the host machine's `localhost`, for running against a locally-hosted API (see [`Backend/README.md`](../Backend/README.md)). For a physical device use the host machine's LAN IP instead; for iOS (once buildable) use `http://localhost:5218`.

### Lint

```bash
flutter analyze
```

Confirmed clean (`No issues found!`) as of this check. There's no CI workflow in this repo wiring this up automatically — it's a manual step for now, not an enforced gate.

### Tests

```bash
flutter test
```

Four test files currently exist under `test/`:

- `test/features/exercises/dio_exercise_repository_test.dart` — `DioExerciseRepository` JSON mapping, error routing, feedback submission
- `test/features/exercises/exercise_notifier_test.dart` — exercise state notifier
- `test/features/appointments/appointments_provider_test.dart` — `appointmentsProvider` success state, `AuthFailure` guard
- `test/features/auth/auth_notifier_test.dart` — `AuthNotifier` login success/failure paths

This is the complete list — most features (profile, RPE feedback UI, slot booking) have no dedicated test coverage yet.

---

## Building a Signed Release APK

A release keystore already exists at [`android/app/physiolink.jks`](android/app/physiolink.jks), referenced by [`android/key.properties`](android/key.properties) (`storeFile=physiolink.jks`, `keyAlias=physiolink`) and wired into the release `signingConfig` in [`android/app/build.gradle.kts`](android/app/build.gradle.kts). **Use this existing keystore — do not generate a new one**, or every device with a prior release install will be unable to install the update (Android rejects a mismatched signature).

```bash
flutter build apk --release --dart-define=BASE_URL=https://physiolink-production.up.railway.app
```

Output: `build/app/outputs/flutter-apk/app-release.apk`.

**Application ID:** `com.physiolink.patient` (`android/app/build.gradle.kts`, `namespace` + `applicationId`). This was renamed from Flutter's default template identifier, `com.example.practice`, the day before Play Store submission planning began — the applicationId becomes permanent once an app is published, and `com.example.practice` is one of the most common placeholder identifiers in existence, both wrong as a permanent production identity and a real collision risk. Anyone with a previously sideloaded build under the old identity must uninstall it first — a different `applicationId` is a different app to Android, not an in-place update.

### Distribution

A signed release APK has been built and distributed for manual testing — sideloaded directly to test devices, not through the Play Store. There is no Play Store listing yet.

---

## Demo Credentials

See the [root README](../README.md#demo-credentials).

---

## Known Limitations / Deferred

- **iOS**: `ios/` is untouched default Flutter scaffolding — no signing configured, bundle identifier still `com.example.practice`. Not started; tracked in the [root README's Roadmap](../README.md#roadmap).
- **No CI**: linting and tests are run manually, not gated by a pipeline.
- **Test coverage**: see [Tests](#tests) above — four files, core auth/exercise/appointment paths only.

---

## Screenshots

_Images go in `docs/screenshots/` (create the folder)._

![Login](docs/screenshots/login.png)
![Home](docs/screenshots/home.png)
![Exercise list](docs/screenshots/exercise-list.png)
![Exercise detail](docs/screenshots/exercise-detail.png)
![Appointments](docs/screenshots/appointments.png)
![Profile](docs/screenshots/profile.png)

---

## License

MIT — see the [root README](../README.md#license).
