import 'package:dio/dio.dart';
import 'package:practice/core/network/api_config.dart';

class DioClient {
  DioClient() {
    _dio = Dio(
      BaseOptions(
        baseUrl: kApiBaseUrl,
        connectTimeout: const Duration(seconds: 10),
        receiveTimeout: const Duration(seconds: 10),
        headers: {'Content-Type': 'application/json'},
      ),
    );
    _dio.interceptors.add(_AuthInterceptor(this));
  }

  late final Dio _dio;
  Dio get dio => _dio;
  String? Function()? tokenGetter; // set after construction
  String? Function()? refreshTokenGetter;
  Future<void> Function(String, String)? tokenUpdater;
  // Called when the refresh token itself is rejected — the session is over.
  void Function()? onSessionExpired;
}

// Endpoints whose 401 means "bad credentials", not "access token expired" —
// refreshing and retrying them can never help.
const _noRefreshPaths = {
  '/api/v1/auth/login',
  '/api/v1/auth/refresh',
  '/api/v1/auth/logout',
};

// Queued: errors are handled one at a time. The refresh token rotates on every
// use, so when several requests 401 together (e.g. the home page's parallel
// loads after the access token expires) only the first may refresh; the rest
// see the new access token already in place and simply retry with it.
class _AuthInterceptor extends QueuedInterceptor {
  final DioClient _client;
  final refreshDio = Dio(
    BaseOptions(
      baseUrl: kApiBaseUrl,
    ),
  );

  _AuthInterceptor(this._client);

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    final token = _client.tokenGetter?.call();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode != 401 ||
        _noRefreshPaths.contains(err.requestOptions.path)) {
      handler.next(err);
      return;
    }

    // Another queued request already refreshed while this one was in flight.
    final sentHeader = err.requestOptions.headers['Authorization'] as String?;
    final currentToken = _client.tokenGetter?.call();
    if (currentToken != null && sentHeader != 'Bearer $currentToken') {
      await _retry(err, handler, currentToken);
      return;
    }

    final refreshToken = _client.refreshTokenGetter?.call();
    if (refreshToken == null) {
      handler.next(err);
      return;
    }

    final String newAccess;
    try {
      final response = await refreshDio.post(
        '/api/v1/auth/refresh',
        data: {'refreshToken': refreshToken},
      );
      newAccess = response.data['accessToken'] as String;
      final newRefresh = response.data['refreshToken'] as String;
      await _client.tokenUpdater?.call(newAccess, newRefresh);
    } on DioException catch (refreshErr) {
      // Refresh token revoked/expired → send the patient back to login rather
      // than leaving every screen failing. Network/5xx errors keep the session.
      if (refreshErr.response?.statusCode == 401) {
        _client.onSessionExpired?.call();
      }
      handler.next(err);
      return;
    } catch (_) {
      handler.next(err);
      return;
    }

    await _retry(err, handler, newAccess);
  }

  Future<void> _retry(
    DioException err,
    ErrorInterceptorHandler handler,
    String accessToken,
  ) async {
    try {
      final retryResponse = await refreshDio.fetch(
        err.requestOptions..headers['Authorization'] = 'Bearer $accessToken',
      );
      handler.resolve(retryResponse);
    } on DioException catch (retryErr) {
      handler.next(retryErr);
    }
  }
}
