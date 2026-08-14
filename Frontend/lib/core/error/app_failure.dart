sealed class AppFailure {
  const AppFailure();
}

class NetworkFailure extends AppFailure {
  const NetworkFailure();
}

class ServerFailure extends AppFailure {
  final int? statusCode;
  const ServerFailure(this.statusCode);
}

// A slot was already taken / no longer bookable (API 409). Extends ServerFailure
// so every existing exhaustive `AppFailure` switch still matches it under the
// `ServerFailure()` arm; the booking screen checks for it explicitly to show a
// "that slot was just taken" message (spec Phase 7).
class SlotUnavailableFailure extends ServerFailure {
  const SlotUnavailableFailure() : super(409);
}

class AuthFailure extends AppFailure {
  const AuthFailure();
}
