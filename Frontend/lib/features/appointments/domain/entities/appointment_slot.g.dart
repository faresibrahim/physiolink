// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'appointment_slot.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_AppointmentSlot _$AppointmentSlotFromJson(Map<String, dynamic> json) =>
    _AppointmentSlot(
      slotId: json['slotId'] as String,
      scheduledAt: DateTime.parse(json['scheduledAt'] as String),
      isAvailable: json['isAvailable'] as bool? ?? true,
    );

Map<String, dynamic> _$AppointmentSlotToJson(_AppointmentSlot instance) =>
    <String, dynamic>{
      'slotId': instance.slotId,
      'scheduledAt': instance.scheduledAt.toIso8601String(),
      'isAvailable': instance.isAvailable,
    };
