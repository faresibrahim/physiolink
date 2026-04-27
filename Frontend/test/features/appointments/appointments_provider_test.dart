import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/domain/repositories/appointments_repository.dart';
import 'package:practice/features/appointments/presentation/providers/appointments_providers.dart';
import 'package:practice/features/auth/presentation/providers/auth_notifier.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';

class MockAppointmentsRepository extends Mock implements AppointmentsRepository {}

class MockFlutterSecureStorage extends Mock implements FlutterSecureStorage {}

class MockDio extends Mock implements Dio {}

class FakeAuthNotifier extends AuthNotifier {
  FakeAuthNotifier() : super(MockFlutterSecureStorage(), MockDio());

  @override
  String? get patientId => 'patient-abc';
}

class FakeAuthNotifierNoPatient extends AuthNotifier {
  FakeAuthNotifierNoPatient() : super(MockFlutterSecureStorage(), MockDio());

  @override
  String? get patientId => null;
}

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
        patientId: 'patient-abc',
        therapistId: 'therapist-1',
        appointmentTime: DateTime(2026, 5, 10, 9, 0),
        status: AppointmentStatus.confirmed,
      );

      when(
        () => mockRepository.getAppointments('patient-abc'),
      ).thenAnswer((_) async => Right([fakeAppointment]));

      final container = ProviderContainer(
        overrides: [
          appointmentsRepositoryProvider.overrideWithValue(mockRepository),
          authNotifierProvider.overrideWith((ref) => FakeAuthNotifier()),
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

    test('returns AuthFailure when patientId is null', () async {
      final container = ProviderContainer(
        overrides: [
          authNotifierProvider.overrideWith((ref) => FakeAuthNotifierNoPatient()),
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
