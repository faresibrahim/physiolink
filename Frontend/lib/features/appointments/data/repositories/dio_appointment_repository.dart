import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/domain/repositories/appointments_repository.dart';

class DioAppointmentRepository implements AppointmentsRepository {
  final Dio _dio;
  DioAppointmentRepository(this._dio);

  @override
  Future<Either<AppFailure, List<Appointment>>> getAppointments(
    String patientId,
  ) async {
    try {
      final response = await _dio.get(
        '/api/v1/patients/$patientId/appointments',
      );
      final appointments = (response.data['items'] as List)
          .map((item) => Appointment.fromJson(_normalize(item)))
          .toList();
      return Right(appointments);
    } catch (ex) {
      return Left(_mapError(ex));
    }
  }

  // Backend's AppointmentDto uses `therapistName`, `appointmentStatus`
  // (serialized as int), and no `therapistId`. Reshape into the JSON the
  // Flutter freezed model expects.
  Map<String, dynamic> _normalize(dynamic item) {
    final map = item as Map<String, dynamic>;
    return {
      'appointmentId': map['appointmentId'],
      'title': map['title'] ?? 'Appointment',
      'patientId': map['patientId'],
      'therapistId': map['therapistName'] ?? '',
      'appointmentTime': map['appointmentTime'],
      'status': _statusFromBackend(map['appointmentStatus']),
      'createdAt': map['createdAt'],
    };
  }

  static const _statusNames = ['Pending', 'Confirmed', 'Cancelled', 'Completed'];

  String _statusFromBackend(dynamic value) {
    if (value is String) return value;
    if (value is int && value >= 0 && value < _statusNames.length) {
      return _statusNames[value];
    }
    return 'Pending';
  }

  @override
  Future<Either<AppFailure, void>> requestAppointment({
    required String patientId,
    required DateTime scheduledAt,
    String? notes,
  }) async {
    try {
      await _dio.post(
        '/api/v1/appointments',
        data: {
          'patientId': patientId,
          'appointmentTime': scheduledAt.toUtc().toIso8601String(),
          if (notes != null && notes.isNotEmpty) 'notes': notes,
        },
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
      // Temporary: print full response to help debug
      // ignore: avoid_print
      print('[AppointmentRepo] ${ex.response?.statusCode} — ${ex.response?.data}');
      return ServerFailure(ex.response?.statusCode ?? 500);
    }
    // ignore: avoid_print
    print('[AppointmentRepo] Non-Dio error: $ex');
    return const ServerFailure(500);
  }
}
