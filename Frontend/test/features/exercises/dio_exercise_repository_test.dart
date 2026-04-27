import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/exercises/data/repositories/dio_exercise_repository.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';

class MockDio extends Mock implements Dio {}

void main() {
  late MockDio mockDio;
  late DioExerciseRepository repository;

  setUp(() {
    mockDio = MockDio();
    repository = DioExerciseRepository(mockDio);
  });

  group('DioExerciseRepository', () {
    test(
      'getExercises maps JSON response to ExerciseAssignment list',
      () async {
        when(() => mockDio.get(any())).thenAnswer(
          (_) async => Response(
            statusCode: 200,
            requestOptions: RequestOptions(path: ''),
            data: {
              'items': [
                {
                  'exerciseAssignmentId': 'ea-1',
                  'exerciseId': 'ex-1',
                  'exerciseName': 'Squat',
                  'description': 'Bend knees to 90 degrees',
                  'difficulty':
                      1, // DifficultyLevel.moderate (index-based enum)
                  'sets': 3,
                  'reps': 12,
                  'durationMinutes': 5,
                  'feedback': null,
                  'completedAt': null,
                  'assignedAt': null,
                },
              ],
            },
          ),
        );

        final result = await repository.getExercises('patient-123');

        expect(result.isRight(), true);
        result.fold((f) => fail('Expected Right but got $f'), (assignments) {
          expect(assignments.length, 1);
          final a = assignments.first;
          expect(a.exerciseAssignmentId, 'ea-1');
          expect(a.exerciseName, 'Squat');
          expect(a.difficulty, DifficultyLevel.moderate);
          expect(a.sets, 3);
          expect(a.reps, 12);
        });
      },
    );

    test(
      'getExercises returns ServerFailure on DioException with status 500',
      () async {
        when(() => mockDio.get(any())).thenThrow(
          DioException(
            requestOptions: RequestOptions(path: ''),
            type: DioExceptionType.badResponse,
            response: Response(
              statusCode: 500,
              requestOptions: RequestOptions(path: ''),
            ),
          ),
        );

        final result = await repository.getExercises('patient-123');

        expect(result.isLeft(), true);
        result.fold((f) {
          expect(f, isA<ServerFailure>());
          expect((f as ServerFailure).statusCode, 500);
        }, (_) => fail('Expected Left but got Right'));
      },
    );

    test('submitFeedback returns Right(null) on success', () async {
      when(() => mockDio.post(any(), data: any(named: 'data'))).thenAnswer(
        (_) async =>
            Response(statusCode: 200, requestOptions: RequestOptions(path: '')),
      );

      final result = await repository.submitFeedback('ex-1', 7);

      expect(result.isRight(), true);
    });
  });
}
