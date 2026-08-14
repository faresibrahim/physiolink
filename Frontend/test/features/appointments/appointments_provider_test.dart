import 'package:dartz/dartz.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/domain/repositories/appointments_repository.dart';
import 'package:practice/features/appointments/presentation/providers/appointments_providers.dart';

class MockAppointmentsRepository extends Mock implements AppointmentsRepository {}

void main() {
  late MockAppointmentsRepository mockRepository;

  setUp(() {
    mockRepository = MockAppointmentsRepository();
  });

  group('appointmentsProvider', () {
    test('returns appointment list on success', () async {
      final fakeAppointment = Appointment(
        appointmentId: 'appt-1',
        title: 'Follow-up session',
        therapistName: 'Dr. Sarah Johnson',
        appointmentTime: DateTime(2026, 5, 10, 9, 0),
        status: AppointmentStatus.confirmed,
        statusLabel: 'Confirmed',
      );

      when(
        () => mockRepository.getMyAppointments(),
      ).thenAnswer((_) async => Right([fakeAppointment]));

      final container = ProviderContainer(
        overrides: [
          appointmentsRepositoryProvider.overrideWithValue(mockRepository),
        ],
      );
      addTearDown(container.dispose);

      final result = await container.read(appointmentsProvider.future);

      expect(result.isRight(), true);
      result.fold(
        (f) => fail('Expected Right but got $f'),
        (appointments) {
          expect(appointments.length, 1);
          expect(appointments.first.appointmentId, 'appt-1');
          expect(appointments.first.status, AppointmentStatus.confirmed);
        },
      );
    });

    test('propagates a repository failure', () async {
      when(
        () => mockRepository.getMyAppointments(),
      ).thenAnswer((_) async => const Left(AuthFailure()));

      final container = ProviderContainer(
        overrides: [
          appointmentsRepositoryProvider.overrideWithValue(mockRepository),
        ],
      );
      addTearDown(container.dispose);

      final result = await container.read(appointmentsProvider.future);

      expect(result.isLeft(), true);
      result.fold(
        (f) => expect(f, isA<AuthFailure>()),
        (_) => fail('Expected Left but got Right'),
      );
    });
  });
}
