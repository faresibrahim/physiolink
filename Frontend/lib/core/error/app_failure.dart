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

class AuthFailure extends AppFailure {
  const AuthFailure();
}
