import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/network/core_providers.dart';
import 'package:practice/features/exercises/data/repositories/dio_exercise_repository.dart';
import 'package:practice/features/exercises/domain/repositories/exercise_repository.dart';

final exerciseRepositoryProvider = Provider<ExerciseRepository>((ref) {
  final dio = ref.watch(dioClientProvider).dio;
  return DioExerciseRepository(dio);
});
