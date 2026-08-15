/// Base URL of the PhysioLink API — the single source of truth for the API host.
///
/// Overridden per build with `--dart-define=BASE_URL=...`. The default targets the
/// Android emulator's host loopback (10.0.2.2 maps to the host machine's localhost);
/// use the PC's LAN IP for a physical device, or http://localhost:5218 for iOS.
const String kApiBaseUrl = String.fromEnvironment(
  'BASE_URL',
  defaultValue: 'http://10.0.2.2:5218',
);
