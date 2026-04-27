import 'package:dartz/dartz.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';

abstract class ExerciseRepository {
  Future<Either<AppFailure, List<ExerciseAssignment>>> getExercises(
    String patientId,
  );

  Future<Either<AppFailure, void>> submitFeedback(
    String exerciseId,
    int? feedback,
  );
}
