// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'exercise_assignment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_ExerciseAssignment _$ExerciseAssignmentFromJson(Map<String, dynamic> json) =>
    _ExerciseAssignment(
      exerciseAssignmentId: json['exerciseAssignmentId'] as String,
      exerciseId: json['exerciseId'] as String,
      exerciseName: json['exerciseName'] as String,
      description: json['description'] as String,
      descriptionAr: json['descriptionAr'] as String?,
      videoUrl: json['videoUrl'] as String?,
      difficulty: $enumDecode(_$DifficultyLevelEnumMap, json['difficulty']),
      sets: (json['sets'] as num).toInt(),
      reps: (json['reps'] as num).toInt(),
      durationMinutes: (json['durationMinutes'] as num).toInt(),
      feedback: (json['feedback'] as num?)?.toInt(),
      completedAt: json['completedAt'] == null
          ? null
          : DateTime.parse(json['completedAt'] as String),
      assignedAt: json['assignedAt'] == null
          ? null
          : DateTime.parse(json['assignedAt'] as String),
    );

Map<String, dynamic> _$ExerciseAssignmentToJson(_ExerciseAssignment instance) =>
    <String, dynamic>{
      'exerciseAssignmentId': instance.exerciseAssignmentId,
      'exerciseId': instance.exerciseId,
      'exerciseName': instance.exerciseName,
      'description': instance.description,
      'descriptionAr': instance.descriptionAr,
      'videoUrl': instance.videoUrl,
      'difficulty': _$DifficultyLevelEnumMap[instance.difficulty]!,
      'sets': instance.sets,
      'reps': instance.reps,
      'durationMinutes': instance.durationMinutes,
      'feedback': instance.feedback,
      'completedAt': instance.completedAt?.toIso8601String(),
      'assignedAt': instance.assignedAt?.toIso8601String(),
    };

const _$DifficultyLevelEnumMap = {
  DifficultyLevel.easy: 'Easy',
  DifficultyLevel.moderate: 'Moderate',
  DifficultyLevel.hard: 'Hard',
};
