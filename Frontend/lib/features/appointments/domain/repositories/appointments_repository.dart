import 'package:dartz/dartz.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';

abstract class AppointmentsRepository {
  Future<Either<AppFailure, List<Appointment>>> getAppointments(
    String patientId,
  );

  Future<Either<AppFailure, void>> requestAppointment({
    required String patientId,
    required DateTime scheduledAt,
    String? notes,
  });
}
