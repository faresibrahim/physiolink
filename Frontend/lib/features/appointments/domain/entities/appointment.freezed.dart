// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'appointment.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$Appointment {

 String get appointmentId; String get title; String? get therapistName; String? get notes; DateTime? get appointmentTime; AppointmentStatus get status;// Honest, server-provided display wording (e.g. "Pending — awaiting
// confirmation"). Kept alongside `status` so the UI never has to imply a
// pending request is confirmed (spec Phase 7).
 String get statusLabel; String? get slotId;
/// Create a copy of Appointment
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$AppointmentCopyWith<Appointment> get copyWith => _$AppointmentCopyWithImpl<Appointment>(this as Appointment, _$identity);

  /// Serializes this Appointment to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is Appointment&&(identical(other.appointmentId, appointmentId) || other.appointmentId == appointmentId)&&(identical(other.title, title) || other.title == title)&&(identical(other.therapistName, therapistName) || other.therapistName == therapistName)&&(identical(other.notes, notes) || other.notes == notes)&&(identical(other.appointmentTime, appointmentTime) || other.appointmentTime == appointmentTime)&&(identical(other.status, status) || other.status == status)&&(identical(other.statusLabel, statusLabel) || other.statusLabel == statusLabel)&&(identical(other.slotId, slotId) || other.slotId == slotId));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,appointmentId,title,therapistName,notes,appointmentTime,status,statusLabel,slotId);

@override
String toString() {
  return 'Appointment(appointmentId: $appointmentId, title: $title, therapistName: $therapistName, notes: $notes, appointmentTime: $appointmentTime, status: $status, statusLabel: $statusLabel, slotId: $slotId)';
}


}

/// @nodoc
abstract mixin class $AppointmentCopyWith<$Res>  {
  factory $AppointmentCopyWith(Appointment value, $Res Function(Appointment) _then) = _$AppointmentCopyWithImpl;
@useResult
$Res call({
 String appointmentId, String title, String? therapistName, String? notes, DateTime? appointmentTime, AppointmentStatus status, String statusLabel, String? slotId
});




}
/// @nodoc
class _$AppointmentCopyWithImpl<$Res>
    implements $AppointmentCopyWith<$Res> {
  _$AppointmentCopyWithImpl(this._self, this._then);

  final Appointment _self;
  final $Res Function(Appointment) _then;

/// Create a copy of Appointment
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? appointmentId = null,Object? title = null,Object? therapistName = freezed,Object? notes = freezed,Object? appointmentTime = freezed,Object? status = null,Object? statusLabel = null,Object? slotId = freezed,}) {
  return _then(_self.copyWith(
appointmentId: null == appointmentId ? _self.appointmentId : appointmentId // ignore: cast_nullable_to_non_nullable
as String,title: null == title ? _self.title : title // ignore: cast_nullable_to_non_nullable
as String,therapistName: freezed == therapistName ? _self.therapistName : therapistName // ignore: cast_nullable_to_non_nullable
as String?,notes: freezed == notes ? _self.notes : notes // ignore: cast_nullable_to_non_nullable
as String?,appointmentTime: freezed == appointmentTime ? _self.appointmentTime : appointmentTime // ignore: cast_nullable_to_non_nullable
as DateTime?,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as AppointmentStatus,statusLabel: null == statusLabel ? _self.statusLabel : statusLabel // ignore: cast_nullable_to_non_nullable
as String,slotId: freezed == slotId ? _self.slotId : slotId // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}

}


/// Adds pattern-matching-related methods to [Appointment].
extension AppointmentPatterns on Appointment {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _Appointment value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Appointment() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _Appointment value)  $default,){
final _that = this;
switch (_that) {
case _Appointment():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _Appointment value)?  $default,){
final _that = this;
switch (_that) {
case _Appointment() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String appointmentId,  String title,  String? therapistName,  String? notes,  DateTime? appointmentTime,  AppointmentStatus status,  String statusLabel,  String? slotId)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Appointment() when $default != null:
return $default(_that.appointmentId,_that.title,_that.therapistName,_that.notes,_that.appointmentTime,_that.status,_that.statusLabel,_that.slotId);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String appointmentId,  String title,  String? therapistName,  String? notes,  DateTime? appointmentTime,  AppointmentStatus status,  String statusLabel,  String? slotId)  $default,) {final _that = this;
switch (_that) {
case _Appointment():
return $default(_that.appointmentId,_that.title,_that.therapistName,_that.notes,_that.appointmentTime,_that.status,_that.statusLabel,_that.slotId);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String appointmentId,  String title,  String? therapistName,  String? notes,  DateTime? appointmentTime,  AppointmentStatus status,  String statusLabel,  String? slotId)?  $default,) {final _that = this;
switch (_that) {
case _Appointment() when $default != null:
return $default(_that.appointmentId,_that.title,_that.therapistName,_that.notes,_that.appointmentTime,_that.status,_that.statusLabel,_that.slotId);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _Appointment implements Appointment {
  const _Appointment({required this.appointmentId, this.title = '', this.therapistName, this.notes, this.appointmentTime, this.status = AppointmentStatus.requested, this.statusLabel = '', this.slotId});
  factory _Appointment.fromJson(Map<String, dynamic> json) => _$AppointmentFromJson(json);

@override final  String appointmentId;
@override@JsonKey() final  String title;
@override final  String? therapistName;
@override final  String? notes;
@override final  DateTime? appointmentTime;
@override@JsonKey() final  AppointmentStatus status;
// Honest, server-provided display wording (e.g. "Pending — awaiting
// confirmation"). Kept alongside `status` so the UI never has to imply a
// pending request is confirmed (spec Phase 7).
@override@JsonKey() final  String statusLabel;
@override final  String? slotId;

/// Create a copy of Appointment
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$AppointmentCopyWith<_Appointment> get copyWith => __$AppointmentCopyWithImpl<_Appointment>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$AppointmentToJson(this, );
}

@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _Appointment&&(identical(other.appointmentId, appointmentId) || other.appointmentId == appointmentId)&&(identical(other.title, title) || other.title == title)&&(identical(other.therapistName, therapistName) || other.therapistName == therapistName)&&(identical(other.notes, notes) || other.notes == notes)&&(identical(other.appointmentTime, appointmentTime) || other.appointmentTime == appointmentTime)&&(identical(other.status, status) || other.status == status)&&(identical(other.statusLabel, statusLabel) || other.statusLabel == statusLabel)&&(identical(other.slotId, slotId) || other.slotId == slotId));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,appointmentId,title,therapistName,notes,appointmentTime,status,statusLabel,slotId);

@override
String toString() {
  return 'Appointment(appointmentId: $appointmentId, title: $title, therapistName: $therapistName, notes: $notes, appointmentTime: $appointmentTime, status: $status, statusLabel: $statusLabel, slotId: $slotId)';
}


}

/// @nodoc
abstract mixin class _$AppointmentCopyWith<$Res> implements $AppointmentCopyWith<$Res> {
  factory _$AppointmentCopyWith(_Appointment value, $Res Function(_Appointment) _then) = __$AppointmentCopyWithImpl;
@override @useResult
$Res call({
 String appointmentId, String title, String? therapistName, String? notes, DateTime? appointmentTime, AppointmentStatus status, String statusLabel, String? slotId
});




}
/// @nodoc
class __$AppointmentCopyWithImpl<$Res>
    implements _$AppointmentCopyWith<$Res> {
  __$AppointmentCopyWithImpl(this._self, this._then);

  final _Appointment _self;
  final $Res Function(_Appointment) _then;

/// Create a copy of Appointment
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? appointmentId = null,Object? title = null,Object? therapistName = freezed,Object? notes = freezed,Object? appointmentTime = freezed,Object? status = null,Object? statusLabel = null,Object? slotId = freezed,}) {
  return _then(_Appointment(
appointmentId: null == appointmentId ? _self.appointmentId : appointmentId // ignore: cast_nullable_to_non_nullable
as String,title: null == title ? _self.title : title // ignore: cast_nullable_to_non_nullable
as String,therapistName: freezed == therapistName ? _self.therapistName : therapistName // ignore: cast_nullable_to_non_nullable
as String?,notes: freezed == notes ? _self.notes : notes // ignore: cast_nullable_to_non_nullable
as String?,appointmentTime: freezed == appointmentTime ? _self.appointmentTime : appointmentTime // ignore: cast_nullable_to_non_nullable
as DateTime?,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as AppointmentStatus,statusLabel: null == statusLabel ? _self.statusLabel : statusLabel // ignore: cast_nullable_to_non_nullable
as String,slotId: freezed == slotId ? _self.slotId : slotId // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

// dart format on
