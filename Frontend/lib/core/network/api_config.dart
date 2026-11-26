/// Base URL of the PhysioLink API — the single source of truth for the API host.
///
/// Defaults to the production API so a plain `flutter build` can never ship a
/// build pointing at a dev machine. For local development override it with
/// `--dart-define=BASE_URL=...`: http://10.0.2.2:5218 for the Android emulator
/// (10.0.2.2 maps to the host's localhost), the PC's LAN IP for a physical
/// device, or http://localhost:5218 for the iOS simulator.
const String kApiBaseUrl = String.fromEnvironment(
  'BASE_URL',
  defaultValue: 'https://physiolink-production.up.railway.app',
);
