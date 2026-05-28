// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'appointment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_Appointment _$AppointmentFromJson(Map<String, dynamic> json) => _Appointment(
  appointmentId: json['appointmentId'] as String,
  title: json['title'] as String,
  patientId: json['patientId'] as String,
  therapistId: json['therapistId'] as String,
  appointmentTime: json['appointmentTime'] == null
      ? null
      : DateTime.parse(json['appointmentTime'] as String),
  status:
      $enumDecodeNullable(_$AppointmentStatusEnumMap, json['status']) ??
      AppointmentStatus.pending,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$AppointmentToJson(_Appointment instance) =>
    <String, dynamic>{
      'appointmentId': instance.appointmentId,
      'title': instance.title,
      'patientId': instance.patientId,
      'therapistId': instance.therapistId,
      'appointmentTime': instance.appointmentTime?.toIso8601String(),
      'status': _$AppointmentStatusEnumMap[instance.status]!,
      'createdAt': instance.createdAt?.toIso8601String(),
    };

const _$AppointmentStatusEnumMap = {
  AppointmentStatus.pending: 'Pending',
  AppointmentStatus.confirmed: 'Confirmed',
  AppointmentStatus.cancelled: 'Cancelled',
  AppointmentStatus.completed: 'Completed',
};
