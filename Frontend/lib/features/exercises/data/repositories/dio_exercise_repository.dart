import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/exercises/domain/repositories/exercise_repository.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';

class DioExerciseRepository implements ExerciseRepository {
  final Dio _dio;
  DioExerciseRepository(this._dio);

  @override
  Future<Either<AppFailure, List<ExerciseAssignment>>> getExercises(
    String patientId,
  ) async {
    try {
      final response = await _dio.get('/api/v1/patients/$patientId/exercises');
      final exercises = (response.data['items'] as List)
          .map((item) => ExerciseAssignment.fromJson(item))
          .toList();
      return Right(exercises);
    } catch (ex) {
      return Left(_mapError(ex));
    }
  }

  @override
  Future<Either<AppFailure, void>> submitFeedback(
    String exerciseId,
    int? feedback,
  ) async {
    try {
      await _dio.post(
        '/api/v1/exercises/$exerciseId/feedback',
        data: {'rating': feedback},
      );
      return const Right(null);
    } catch (ex) {
      return Left(_mapError(ex));
    }
  }

  AppFailure _mapError(Object ex) {
    if (ex is DioException) {
      if (ex.type == DioExceptionType.connectionError)
        return const NetworkFailure();
      if (ex.response?.statusCode == 401) return const AuthFailure();
      return ServerFailure(ex.response?.statusCode ?? 500);
    }
    return const ServerFailure(500);
  }
}
