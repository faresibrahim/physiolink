import 'package:dartz/dartz.dart';
import 'package:practice/core/error/app_failure.dart';

abstract class NotificationRepository {
  Future<Either<AppFailure, void>> registerDeviceToken(String token);
}
