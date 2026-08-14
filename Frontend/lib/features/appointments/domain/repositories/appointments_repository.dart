import 'package:dartz/dartz.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';

abstract class AppointmentsRepository {
  // The signed-in patient's appointments, incl. pending/rejected/expired/
  // cancelled state (spec Phase 5). Patient is resolved from the JWT.
  Future<Either<AppFailure, List<Appointment>>> getMyAppointments();
}
