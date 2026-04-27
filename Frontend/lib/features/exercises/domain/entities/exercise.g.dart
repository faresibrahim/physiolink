// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'exercise.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_Exercise _$ExerciseFromJson(Map<String, dynamic> json) => _Exercise(
  id: json['id'] as String,
  name: json['name'] as String,
  sets: (json['sets'] as num).toInt(),
  reps: (json['reps'] as num).toInt(),
  durationMinutes: (json['durationMinutes'] as num).toInt(),
  difficulty: json['difficulty'] as String,
  description: json['description'] as String,
  isComplete: json['isComplete'] as bool? ?? false,
);

Map<String, dynamic> _$ExerciseToJson(_Exercise instance) => <String, dynamic>{
  'id': instance.id,
  'name': instance.name,
  'sets': instance.sets,
  'reps': instance.reps,
  'durationMinutes': instance.durationMinutes,
  'difficulty': instance.difficulty,
  'description': instance.description,
  'isComplete': instance.isComplete,
};
