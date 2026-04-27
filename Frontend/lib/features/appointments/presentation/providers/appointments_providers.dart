import 'package:dartz/dartz.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/network/core_providers.dart';
import 'package:practice/features/appointments/data/repositories/dio_appointment_repository.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/domain/repositories/appointments_repository.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';

final appointmentsRepositoryProvider = Provider<AppointmentsRepository>((ref) {
  final dio = ref.watch(dioClientProvider).dio;
  return DioAppointmentRepository(dio);
});

final appointmentsProvider =
    FutureProvider<Either<AppFailure, List<Appointment>>>((ref) async {
      final patientId = ref.read(authNotifierProvider).patientId;
      if (patientId == null) return Left(AuthFailure());

      final repository = ref.watch(appointmentsRepositoryProvider);
      return repository.getAppointments(patientId);
    });
