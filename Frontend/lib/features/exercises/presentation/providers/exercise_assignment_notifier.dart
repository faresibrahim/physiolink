import 'package:flutter_riverpod/legacy.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';

class ExerciseAssignmentNotifier
    extends StateNotifier<List<ExerciseAssignment>> {
  ExerciseAssignmentNotifier() : super([]);

  void onFeedbackSelected(String assignmentId, int? feedback) {
    final index = state.indexWhere(
      (a) => a.exerciseAssignmentId == assignmentId,
    );
    if (index != -1) {
      state = [
        ...state.sublist(0, index),
        state[index].copyWith(feedback: feedback),
        ...state.sublist(index + 1),
      ];
    }
  }
}

final exerciseAssignmentProvider =
    StateNotifierProvider<ExerciseAssignmentNotifier, List<ExerciseAssignment>>(
      (ref) => ExerciseAssignmentNotifier(),
    );
