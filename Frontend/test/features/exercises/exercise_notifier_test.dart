import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';
import 'package:practice/features/exercises/domain/repositories/exercise_repository.dart';
import 'package:practice/features/exercises/presentation/providers/exercise_fetch_provider.dart';
import 'package:practice/features/exercises/presentation/providers/providers.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';
import 'package:practice/features/auth/presentation/providers/auth_notifier.dart';

class MockExerciseRepository extends Mock implements ExerciseRepository {}

class MockFlutterSecureStorage extends Mock implements FlutterSecureStorage {}

class MockDio extends Mock implements Dio {}

class FakeAuthNotifier extends AuthNotifier {
  FakeAuthNotifier() : super(MockFlutterSecureStorage(), MockDio());

  @override
  String? get patientId => 'test-patient-id';
}

class FakeAuthNotifierNoPatient extends AuthNotifier {
  FakeAuthNotifierNoPatient() : super(MockFlutterSecureStorage(), MockDio());

  @override
  String? get patientId => null;
}

void main() {
  late MockExerciseRepository mockRepository;

  setUp(() {
    mockRepository = MockExerciseRepository();
  });

  group('exercisesProvider', () {
    test('returns exercises when patientId is available', () async {
      final fakeAssignment = ExerciseAssignment(
        exerciseAssignmentId: '1',
        exerciseId: '2',
        exerciseName: 'Squat',
        description: 'Lower body until knees reach 90 degrees',
        difficulty: DifficultyLevel.moderate,
        sets: 3,
        reps: 12,
        durationMinutes: 5,
      );

      when(
        () => mockRepository.getExercises('test-patient-id'),
      ).thenAnswer((_) async => Right([fakeAssignment]));

      final container = ProviderContainer(
        overrides: [
          exerciseRepositoryProvider.overrideWithValue(mockRepository),
          authNotifierProvider.overrideWith((ref) => FakeAuthNotifier()),
        ],
      );

      final result = await container.read(exercisesProvider.future);
      expect(result.isRight(), true);
      result.fold(
        (failure) => fail('Expected Right but got Left'),
        (assignments) => expect(assignments, [fakeAssignment]),
      );
    });
    test('return AuthFailure when patientId is null', () async {
      //We don't need when()
      final container = ProviderContainer(
        overrides: [
          authNotifierProvider.overrideWith(
            (ref) => FakeAuthNotifierNoPatient(),
          ),
        ],
      );
      final result = await container.read(exercisesProvider.future);
      expect(result.isLeft(), true);
    });

    test('returns left when repository fails', () async {
      when(
        () => mockRepository.getExercises('test-patient-id'),
      ).thenAnswer((_) async => Left(NetworkFailure()));

      final container = ProviderContainer(
        overrides: [
          exerciseRepositoryProvider.overrideWithValue(mockRepository),
          authNotifierProvider.overrideWith((ref) => FakeAuthNotifier()),
        ],
      );

      final failure = await container.read(exercisesProvider.future);
      expect(failure.isLeft(), true);
      failure.fold(
        (failure) => expect(failure, isA<NetworkFailure>()),
        (_) => fail('Expected Left but got Right'),
      );
    });
  });
}
