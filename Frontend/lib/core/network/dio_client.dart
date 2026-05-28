import 'package:dio/dio.dart';

class DioClient {
  DioClient() {
    _dio = Dio(
      BaseOptions(
        baseUrl: String.fromEnvironment(
          'BASE_URL',
          defaultValue: 'http://192.168.1.58:5218',
        ),
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
  void Function(String, String)? tokenUpdater;
}

class _AuthInterceptor extends Interceptor {
  final DioClient _client;
  final refreshDio = Dio(
    BaseOptions(
      baseUrl: String.fromEnvironment(
        'BASE_URL',
        defaultValue: 'http://192.168.1.58:5218',
      ),
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

  //When the accessToken expires, the interceptor needs to handle this situation silently.
  //We create a separate dio for this situation, we check for 401 error
  //if the error is 401, create a new refresh and access token accordingly and attach them.
  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode != 401) {
      //No 401, user is authorized, proceed
      handler.next(err);
      return;
    }

    try {
      final refreshToken = _client.refreshTokenGetter?.call();
      if (refreshToken == null) {
        handler.next(err);
        return;
      }
      final response = await refreshDio.post(
        '/api/v1/auth/refresh',
        data: {'refreshToken': refreshToken},
      );
      final newAccess = response.data['accessToken'] as String;
      final newRefresh = response.data['refreshToken'] as String;
      _client.tokenUpdater?.call(newAccess, newRefresh);

      final retryResponse = await refreshDio.fetch(
        err.requestOptions..headers['Authorization'] = 'Bearer $newAccess',
      );
      handler.resolve(retryResponse);
    } catch (ex) {
      handler.next(err);
    }
  }
}
