import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/domain/repositories/appointments_repository.dart';

class DioAppointmentRepository implements AppointmentsRepository {
  final Dio _dio;
  DioAppointmentRepository(this._dio);

  @override
  Future<Either<AppFailure, List<Appointment>>> getMyAppointments() async {
    try {
      final response = await _dio.get('/api/v1/patients/me/appointments');
      // The /me endpoint returns a bare JSON array of PatientAppointmentDto.
      final appointments = (response.data as List)
          .map((item) => Appointment.fromJson(_normalize(item)))
          .toList();
      return Right(appointments);
    } catch (ex) {
      return Left(_mapError(ex));
    }
  }

  // Reshape the API's PatientAppointmentDto into the JSON the freezed model
  // expects (scheduledAt -> appointmentTime, type -> title).
  Map<String, dynamic> _normalize(dynamic item) {
    final map = item as Map<String, dynamic>;
    return {
      'appointmentId': map['appointmentId'],
      'title': map['type'] ?? 'Appointment',
      'therapistName': map['therapistName'],
      'notes': map['notes'],
      'appointmentTime': map['scheduledAt'],
      'status': map['status'] ?? 'Requested',
      'statusLabel': map['statusLabel'] ?? '',
      'slotId': map['slotId'],
    };
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
