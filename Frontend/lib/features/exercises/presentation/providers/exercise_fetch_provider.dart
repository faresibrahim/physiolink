import 'package:dartz/dartz.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/exercises/presentation/providers/providers.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';

final exercisesProvider =
    FutureProvider<Either<AppFailure, List<ExerciseAssignment>>>((ref) async {
      final patient = ref.read(authNotifierProvider).patientId;
      final repository = ref.watch(
        exerciseRepositoryProvider,
      ); // ref is valid here
      if (patient == null) return Left(AuthFailure());

      return await repository.getExercises(patient);
    });
