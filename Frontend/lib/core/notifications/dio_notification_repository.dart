import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/notifications/notification_repository.dart';

class DioNotificationRepository implements NotificationRepository {
  DioNotificationRepository(this._dio);
  final Dio _dio;

  @override
  Future<Either<AppFailure, void>> registerDeviceToken(String token) async {
    try {
      await _dio.patch(
        '/api/v1/patients/me/device-token',
        data: {'deviceToken': token},
      );
      return const Right(null);
    } catch (ex) {
      return Left(_mapError(ex));
    }
  }

  AppFailure _mapError(Object ex) {
    if (ex is DioException) {
      if (ex.type == DioExceptionType.connectionError) {
        return const NetworkFailure();
      }
      if (ex.response?.statusCode == 401) return const AuthFailure();
      return ServerFailure(ex.response?.statusCode ?? 500);
    }
    return const ServerFailure(500);
  }
}
