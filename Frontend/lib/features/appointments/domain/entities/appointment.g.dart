// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'appointment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_Appointment _$AppointmentFromJson(Map<String, dynamic> json) => _Appointment(
  appointmentId: json['appointmentId'] as String,
  title: json['title'] as String? ?? '',
  therapistName: json['therapistName'] as String?,
  notes: json['notes'] as String?,
  appointmentTime: json['appointmentTime'] == null
      ? null
      : DateTime.parse(json['appointmentTime'] as String),
  status:
      $enumDecodeNullable(_$AppointmentStatusEnumMap, json['status']) ??
      AppointmentStatus.requested,
  statusLabel: json['statusLabel'] as String? ?? '',
  slotId: json['slotId'] as String?,
);

Map<String, dynamic> _$AppointmentToJson(_Appointment instance) =>
    <String, dynamic>{
      'appointmentId': instance.appointmentId,
      'title': instance.title,
      'therapistName': instance.therapistName,
      'notes': instance.notes,
      'appointmentTime': instance.appointmentTime?.toIso8601String(),
      'status': _$AppointmentStatusEnumMap[instance.status]!,
      'statusLabel': instance.statusLabel,
      'slotId': instance.slotId,
    };

const _$AppointmentStatusEnumMap = {
  AppointmentStatus.requested: 'Requested',
  AppointmentStatus.confirmed: 'Confirmed',
  AppointmentStatus.completed: 'Completed',
  AppointmentStatus.rejected: 'Rejected',
  AppointmentStatus.expired: 'Expired',
  AppointmentStatus.cancelledByClinic: 'CancelledByClinic',
};
