import 'package:flutter_riverpod/legacy.dart';
import 'package:practice/features/exercises/domain/entities/exercise.dart';

class ExerciseNotifier extends StateNotifier<List<Exercise>> {
  ExerciseNotifier() : super([]);
  //this method should never return a list, always void. StateNotifier manages the state internally, we don't return it.
}

final exerciseProvider =
    StateNotifierProvider<ExerciseNotifier, List<Exercise>>(
      (ref) => ExerciseNotifier(),
    );
