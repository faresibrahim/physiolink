// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'exercise_assignment.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$ExerciseAssignment {

 String get exerciseAssignmentId; String get exerciseId; String get exerciseName; String get description;/// Arabic translation of [description]. Null when the exercise has no
/// translation yet — the UI falls back to [description].
 String? get descriptionAr; String? get videoUrl; DifficultyLevel get difficulty; int get sets; int get reps; int get durationMinutes; int? get feedback; DateTime? get completedAt; DateTime? get assignedAt;
/// Create a copy of ExerciseAssignment
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ExerciseAssignmentCopyWith<ExerciseAssignment> get copyWith => _$ExerciseAssignmentCopyWithImpl<ExerciseAssignment>(this as ExerciseAssignment, _$identity);

  /// Serializes this ExerciseAssignment to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ExerciseAssignment&&(identical(other.exerciseAssignmentId, exerciseAssignmentId) || other.exerciseAssignmentId == exerciseAssignmentId)&&(identical(other.exerciseId, exerciseId) || other.exerciseId == exerciseId)&&(identical(other.exerciseName, exerciseName) || other.exerciseName == exerciseName)&&(identical(other.description, description) || other.description == description)&&(identical(other.descriptionAr, descriptionAr) || other.descriptionAr == descriptionAr)&&(identical(other.videoUrl, videoUrl) || other.videoUrl == videoUrl)&&(identical(other.difficulty, difficulty) || other.difficulty == difficulty)&&(identical(other.sets, sets) || other.sets == sets)&&(identical(other.reps, reps) || other.reps == reps)&&(identical(other.durationMinutes, durationMinutes) || other.durationMinutes == durationMinutes)&&(identical(other.feedback, feedback) || other.feedback == feedback)&&(identical(other.completedAt, completedAt) || other.completedAt == completedAt)&&(identical(other.assignedAt, assignedAt) || other.assignedAt == assignedAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,exerciseAssignmentId,exerciseId,exerciseName,description,descriptionAr,videoUrl,difficulty,sets,reps,durationMinutes,feedback,completedAt,assignedAt);

@override
String toString() {
  return 'ExerciseAssignment(exerciseAssignmentId: $exerciseAssignmentId, exerciseId: $exerciseId, exerciseName: $exerciseName, description: $description, descriptionAr: $descriptionAr, videoUrl: $videoUrl, difficulty: $difficulty, sets: $sets, reps: $reps, durationMinutes: $durationMinutes, feedback: $feedback, completedAt: $completedAt, assignedAt: $assignedAt)';
}


}

/// @nodoc
abstract mixin class $ExerciseAssignmentCopyWith<$Res>  {
  factory $ExerciseAssignmentCopyWith(ExerciseAssignment value, $Res Function(ExerciseAssignment) _then) = _$ExerciseAssignmentCopyWithImpl;
@useResult
$Res call({
 String exerciseAssignmentId, String exerciseId, String exerciseName, String description, String? descriptionAr, String? videoUrl, DifficultyLevel difficulty, int sets, int reps, int durationMinutes, int? feedback, DateTime? completedAt, DateTime? assignedAt
});




}
/// @nodoc
class _$ExerciseAssignmentCopyWithImpl<$Res>
    implements $ExerciseAssignmentCopyWith<$Res> {
  _$ExerciseAssignmentCopyWithImpl(this._self, this._then);

  final ExerciseAssignment _self;
  final $Res Function(ExerciseAssignment) _then;

/// Create a copy of ExerciseAssignment
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? exerciseAssignmentId = null,Object? exerciseId = null,Object? exerciseName = null,Object? description = null,Object? descriptionAr = freezed,Object? videoUrl = freezed,Object? difficulty = null,Object? sets = null,Object? reps = null,Object? durationMinutes = null,Object? feedback = freezed,Object? completedAt = freezed,Object? assignedAt = freezed,}) {
  return _then(_self.copyWith(
exerciseAssignmentId: null == exerciseAssignmentId ? _self.exerciseAssignmentId : exerciseAssignmentId // ignore: cast_nullable_to_non_nullable
as String,exerciseId: null == exerciseId ? _self.exerciseId : exerciseId // ignore: cast_nullable_to_non_nullable
as String,exerciseName: null == exerciseName ? _self.exerciseName : exerciseName // ignore: cast_nullable_to_non_nullable
as String,description: null == description ? _self.description : description // ignore: cast_nullable_to_non_nullable
as String,descriptionAr: freezed == descriptionAr ? _self.descriptionAr : descriptionAr // ignore: cast_nullable_to_non_nullable
as String?,videoUrl: freezed == videoUrl ? _self.videoUrl : videoUrl // ignore: cast_nullable_to_non_nullable
as String?,difficulty: null == difficulty ? _self.difficulty : difficulty // ignore: cast_nullable_to_non_nullable
as DifficultyLevel,sets: null == sets ? _self.sets : sets // ignore: cast_nullable_to_non_nullable
as int,reps: null == reps ? _self.reps : reps // ignore: cast_nullable_to_non_nullable
as int,durationMinutes: null == durationMinutes ? _self.durationMinutes : durationMinutes // ignore: cast_nullable_to_non_nullable
as int,feedback: freezed == feedback ? _self.feedback : feedback // ignore: cast_nullable_to_non_nullable
as int?,completedAt: freezed == completedAt ? _self.completedAt : completedAt // ignore: cast_nullable_to_non_nullable
as DateTime?,assignedAt: freezed == assignedAt ? _self.assignedAt : assignedAt // ignore: cast_nullable_to_non_nullable
as DateTime?,
  ));
}

}


/// Adds pattern-matching-related methods to [ExerciseAssignment].
extension ExerciseAssignmentPatterns on ExerciseAssignment {
/// A variant of `map` that fallback to returning `orElse`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _ExerciseAssignment value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _ExerciseAssignment() when $default != null:
return $default(_that);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// Callbacks receives the raw object, upcasted.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case final Subclass2 value:
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _ExerciseAssignment value)  $default,){
final _that = this;
switch (_that) {
case _ExerciseAssignment():
return $default(_that);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `map` that fallback to returning `null`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _ExerciseAssignment value)?  $default,){
final _that = this;
switch (_that) {
case _ExerciseAssignment() when $default != null:
return $default(_that);case _:
  return null;

}
}
/// A variant of `when` that fallback to an `orElse` callback.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String exerciseAssignmentId,  String exerciseId,  String exerciseName,  String description,  String? descriptionAr,  String? videoUrl,  DifficultyLevel difficulty,  int sets,  int reps,  int durationMinutes,  int? feedback,  DateTime? completedAt,  DateTime? assignedAt)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _ExerciseAssignment() when $default != null:
return $default(_that.exerciseAssignmentId,_that.exerciseId,_that.exerciseName,_that.description,_that.descriptionAr,_that.videoUrl,_that.difficulty,_that.sets,_that.reps,_that.durationMinutes,_that.feedback,_that.completedAt,_that.assignedAt);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// As opposed to `map`, this offers destructuring.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case Subclass2(:final field2):
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String exerciseAssignmentId,  String exerciseId,  String exerciseName,  String description,  String? descriptionAr,  String? videoUrl,  DifficultyLevel difficulty,  int sets,  int reps,  int durationMinutes,  int? feedback,  DateTime? completedAt,  DateTime? assignedAt)  $default,) {final _that = this;
switch (_that) {
case _ExerciseAssignment():
return $default(_that.exerciseAssignmentId,_that.exerciseId,_that.exerciseName,_that.description,_that.descriptionAr,_that.videoUrl,_that.difficulty,_that.sets,_that.reps,_that.durationMinutes,_that.feedback,_that.completedAt,_that.assignedAt);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `when` that fallback to returning `null`
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String exerciseAssignmentId,  String exerciseId,  String exerciseName,  String description,  String? descriptionAr,  String? videoUrl,  DifficultyLevel difficulty,  int sets,  int reps,  int durationMinutes,  int? feedback,  DateTime? completedAt,  DateTime? assignedAt)?  $default,) {final _that = this;
switch (_that) {
case _ExerciseAssignment() when $default != null:
return $default(_that.exerciseAssignmentId,_that.exerciseId,_that.exerciseName,_that.description,_that.descriptionAr,_that.videoUrl,_that.difficulty,_that.sets,_that.reps,_that.durationMinutes,_that.feedback,_that.completedAt,_that.assignedAt);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _ExerciseAssignment implements ExerciseAssignment {
  const _ExerciseAssignment({required this.exerciseAssignmentId, required this.exerciseId, required this.exerciseName, required this.description, this.descriptionAr, this.videoUrl, required this.difficulty, required this.sets, required this.reps, required this.durationMinutes, this.feedback, this.completedAt, this.assignedAt});
  factory _ExerciseAssignment.fromJson(Map<String, dynamic> json) => _$ExerciseAssignmentFromJson(json);

@override final  String exerciseAssignmentId;
@override final  String exerciseId;
@override final  String exerciseName;
@override final  String description;
/// Arabic translation of [description]. Null when the exercise has no
/// translation yet — the UI falls back to [description].
@override final  String? descriptionAr;
@override final  String? videoUrl;
@override final  DifficultyLevel difficulty;
@override final  int sets;
@override final  int reps;
@override final  int durationMinutes;
@override final  int? feedback;
@override final  DateTime? completedAt;
@override final  DateTime? assignedAt;

/// Create a copy of ExerciseAssignment
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ExerciseAssignmentCopyWith<_ExerciseAssignment> get copyWith => __$ExerciseAssignmentCopyWithImpl<_ExerciseAssignment>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ExerciseAssignmentToJson(this, );
}

@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _ExerciseAssignment&&(identical(other.exerciseAssignmentId, exerciseAssignmentId) || other.exerciseAssignmentId == exerciseAssignmentId)&&(identical(other.exerciseId, exerciseId) || other.exerciseId == exerciseId)&&(identical(other.exerciseName, exerciseName) || other.exerciseName == exerciseName)&&(identical(other.description, description) || other.description == description)&&(identical(other.descriptionAr, descriptionAr) || other.descriptionAr == descriptionAr)&&(identical(other.videoUrl, videoUrl) || other.videoUrl == videoUrl)&&(identical(other.difficulty, difficulty) || other.difficulty == difficulty)&&(identical(other.sets, sets) || other.sets == sets)&&(identical(other.reps, reps) || other.reps == reps)&&(identical(other.durationMinutes, durationMinutes) || other.durationMinutes == durationMinutes)&&(identical(other.feedback, feedback) || other.feedback == feedback)&&(identical(other.completedAt, completedAt) || other.completedAt == completedAt)&&(identical(other.assignedAt, assignedAt) || other.assignedAt == assignedAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,exerciseAssignmentId,exerciseId,exerciseName,description,descriptionAr,videoUrl,difficulty,sets,reps,durationMinutes,feedback,completedAt,assignedAt);

@override
String toString() {
  return 'ExerciseAssignment(exerciseAssignmentId: $exerciseAssignmentId, exerciseId: $exerciseId, exerciseName: $exerciseName, description: $description, descriptionAr: $descriptionAr, videoUrl: $videoUrl, difficulty: $difficulty, sets: $sets, reps: $reps, durationMinutes: $durationMinutes, feedback: $feedback, completedAt: $completedAt, assignedAt: $assignedAt)';
}


}

/// @nodoc
abstract mixin class _$ExerciseAssignmentCopyWith<$Res> implements $ExerciseAssignmentCopyWith<$Res> {
  factory _$ExerciseAssignmentCopyWith(_ExerciseAssignment value, $Res Function(_ExerciseAssignment) _then) = __$ExerciseAssignmentCopyWithImpl;
@override @useResult
$Res call({
 String exerciseAssignmentId, String exerciseId, String exerciseName, String description, String? descriptionAr, String? videoUrl, DifficultyLevel difficulty, int sets, int reps, int durationMinutes, int? feedback, DateTime? completedAt, DateTime? assignedAt
});




}
/// @nodoc
class __$ExerciseAssignmentCopyWithImpl<$Res>
    implements _$ExerciseAssignmentCopyWith<$Res> {
  __$ExerciseAssignmentCopyWithImpl(this._self, this._then);

  final _ExerciseAssignment _self;
  final $Res Function(_ExerciseAssignment) _then;

/// Create a copy of ExerciseAssignment
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? exerciseAssignmentId = null,Object? exerciseId = null,Object? exerciseName = null,Object? description = null,Object? descriptionAr = freezed,Object? videoUrl = freezed,Object? difficulty = null,Object? sets = null,Object? reps = null,Object? durationMinutes = null,Object? feedback = freezed,Object? completedAt = freezed,Object? assignedAt = freezed,}) {
  return _then(_ExerciseAssignment(
exerciseAssignmentId: null == exerciseAssignmentId ? _self.exerciseAssignmentId : exerciseAssignmentId // ignore: cast_nullable_to_non_nullable
as String,exerciseId: null == exerciseId ? _self.exerciseId : exerciseId // ignore: cast_nullable_to_non_nullable
as String,exerciseName: null == exerciseName ? _self.exerciseName : exerciseName // ignore: cast_nullable_to_non_nullable
as String,description: null == description ? _self.description : description // ignore: cast_nullable_to_non_nullable
as String,descriptionAr: freezed == descriptionAr ? _self.descriptionAr : descriptionAr // ignore: cast_nullable_to_non_nullable
as String?,videoUrl: freezed == videoUrl ? _self.videoUrl : videoUrl // ignore: cast_nullable_to_non_nullable
as String?,difficulty: null == difficulty ? _self.difficulty : difficulty // ignore: cast_nullable_to_non_nullable
as DifficultyLevel,sets: null == sets ? _self.sets : sets // ignore: cast_nullable_to_non_nullable
as int,reps: null == reps ? _self.reps : reps // ignore: cast_nullable_to_non_nullable
as int,durationMinutes: null == durationMinutes ? _self.durationMinutes : durationMinutes // ignore: cast_nullable_to_non_nullable
as int,feedback: freezed == feedback ? _self.feedback : feedback // ignore: cast_nullable_to_non_nullable
as int?,completedAt: freezed == completedAt ? _self.completedAt : completedAt // ignore: cast_nullable_to_non_nullable
as DateTime?,assignedAt: freezed == assignedAt ? _self.assignedAt : assignedAt // ignore: cast_nullable_to_non_nullable
as DateTime?,
  ));
}


}

// dart format on
