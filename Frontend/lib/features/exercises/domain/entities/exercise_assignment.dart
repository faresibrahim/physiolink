import 'package:freezed_annotation/freezed_annotation.dart';

part 'exercise_assignment.freezed.dart';
part 'exercise_assignment.g.dart';

enum DifficultyLevel {
  @JsonValue('Easy')
  easy,
  @JsonValue('Moderate')
  moderate,
  @JsonValue('Hard')
  hard,
}

@freezed
abstract class ExerciseAssignment with _$ExerciseAssignment {
  const factory ExerciseAssignment({
    required String exerciseAssignmentId,
    required String exerciseId,
    required String exerciseName,
    required String description,

    /// Arabic translation of [description]. Null when the exercise has no
    /// translation yet — the UI falls back to [description].
    String? descriptionAr,
    String? videoUrl,
    required DifficultyLevel difficulty,
    required int sets,
    required int reps,
    required int durationMinutes,
    int? feedback,
    DateTime? completedAt,
    DateTime? assignedAt,
  }) = _ExerciseAssignment;

  factory ExerciseAssignment.fromJson(Map<String, dynamic> json) =>
      _$ExerciseAssignmentFromJson(json);
}
