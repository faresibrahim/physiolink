// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'appointment_slot.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$AppointmentSlot {

 String get slotId; DateTime get scheduledAt;// False when the slot is already taken (Requested/Booked). Such slots are
// still shown in the calendar as "Booked" but are not tappable. Defaults to
// true so older payloads without the field read as available.
 bool get isAvailable;
/// Create a copy of AppointmentSlot
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$AppointmentSlotCopyWith<AppointmentSlot> get copyWith => _$AppointmentSlotCopyWithImpl<AppointmentSlot>(this as AppointmentSlot, _$identity);

  /// Serializes this AppointmentSlot to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is AppointmentSlot&&(identical(other.slotId, slotId) || other.slotId == slotId)&&(identical(other.scheduledAt, scheduledAt) || other.scheduledAt == scheduledAt)&&(identical(other.isAvailable, isAvailable) || other.isAvailable == isAvailable));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,slotId,scheduledAt,isAvailable);

@override
String toString() {
  return 'AppointmentSlot(slotId: $slotId, scheduledAt: $scheduledAt, isAvailable: $isAvailable)';
}


}

/// @nodoc
abstract mixin class $AppointmentSlotCopyWith<$Res>  {
  factory $AppointmentSlotCopyWith(AppointmentSlot value, $Res Function(AppointmentSlot) _then) = _$AppointmentSlotCopyWithImpl;
@useResult
$Res call({
 String slotId, DateTime scheduledAt, bool isAvailable
});




}
/// @nodoc
class _$AppointmentSlotCopyWithImpl<$Res>
    implements $AppointmentSlotCopyWith<$Res> {
  _$AppointmentSlotCopyWithImpl(this._self, this._then);

  final AppointmentSlot _self;
  final $Res Function(AppointmentSlot) _then;

/// Create a copy of AppointmentSlot
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? slotId = null,Object? scheduledAt = null,Object? isAvailable = null,}) {
  return _then(_self.copyWith(
slotId: null == slotId ? _self.slotId : slotId // ignore: cast_nullable_to_non_nullable
as String,scheduledAt: null == scheduledAt ? _self.scheduledAt : scheduledAt // ignore: cast_nullable_to_non_nullable
as DateTime,isAvailable: null == isAvailable ? _self.isAvailable : isAvailable // ignore: cast_nullable_to_non_nullable
as bool,
  ));
}

}


/// Adds pattern-matching-related methods to [AppointmentSlot].
extension AppointmentSlotPatterns on AppointmentSlot {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _AppointmentSlot value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _AppointmentSlot() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _AppointmentSlot value)  $default,){
final _that = this;
switch (_that) {
case _AppointmentSlot():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _AppointmentSlot value)?  $default,){
final _that = this;
switch (_that) {
case _AppointmentSlot() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String slotId,  DateTime scheduledAt,  bool isAvailable)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _AppointmentSlot() when $default != null:
return $default(_that.slotId,_that.scheduledAt,_that.isAvailable);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String slotId,  DateTime scheduledAt,  bool isAvailable)  $default,) {final _that = this;
switch (_that) {
case _AppointmentSlot():
return $default(_that.slotId,_that.scheduledAt,_that.isAvailable);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String slotId,  DateTime scheduledAt,  bool isAvailable)?  $default,) {final _that = this;
switch (_that) {
case _AppointmentSlot() when $default != null:
return $default(_that.slotId,_that.scheduledAt,_that.isAvailable);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _AppointmentSlot implements AppointmentSlot {
  const _AppointmentSlot({required this.slotId, required this.scheduledAt, this.isAvailable = true});
  factory _AppointmentSlot.fromJson(Map<String, dynamic> json) => _$AppointmentSlotFromJson(json);

@override final  String slotId;
@override final  DateTime scheduledAt;
// False when the slot is already taken (Requested/Booked). Such slots are
// still shown in the calendar as "Booked" but are not tappable. Defaults to
// true so older payloads without the field read as available.
@override@JsonKey() final  bool isAvailable;

/// Create a copy of AppointmentSlot
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$AppointmentSlotCopyWith<_AppointmentSlot> get copyWith => __$AppointmentSlotCopyWithImpl<_AppointmentSlot>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$AppointmentSlotToJson(this, );
}

@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _AppointmentSlot&&(identical(other.slotId, slotId) || other.slotId == slotId)&&(identical(other.scheduledAt, scheduledAt) || other.scheduledAt == scheduledAt)&&(identical(other.isAvailable, isAvailable) || other.isAvailable == isAvailable));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,slotId,scheduledAt,isAvailable);

@override
String toString() {
  return 'AppointmentSlot(slotId: $slotId, scheduledAt: $scheduledAt, isAvailable: $isAvailable)';
}


}

/// @nodoc
abstract mixin class _$AppointmentSlotCopyWith<$Res> implements $AppointmentSlotCopyWith<$Res> {
  factory _$AppointmentSlotCopyWith(_AppointmentSlot value, $Res Function(_AppointmentSlot) _then) = __$AppointmentSlotCopyWithImpl;
@override @useResult
$Res call({
 String slotId, DateTime scheduledAt, bool isAvailable
});




}
/// @nodoc
class __$AppointmentSlotCopyWithImpl<$Res>
    implements _$AppointmentSlotCopyWith<$Res> {
  __$AppointmentSlotCopyWithImpl(this._self, this._then);

  final _AppointmentSlot _self;
  final $Res Function(_AppointmentSlot) _then;

/// Create a copy of AppointmentSlot
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? slotId = null,Object? scheduledAt = null,Object? isAvailable = null,}) {
  return _then(_AppointmentSlot(
slotId: null == slotId ? _self.slotId : slotId // ignore: cast_nullable_to_non_nullable
as String,scheduledAt: null == scheduledAt ? _self.scheduledAt : scheduledAt // ignore: cast_nullable_to_non_nullable
as DateTime,isAvailable: null == isAvailable ? _self.isAvailable : isAvailable // ignore: cast_nullable_to_non_nullable
as bool,
  ));
}


}

// dart format on
