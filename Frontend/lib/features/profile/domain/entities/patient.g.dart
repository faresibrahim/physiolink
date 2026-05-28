// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'patient.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_Patient _$PatientFromJson(Map<String, dynamic> json) => _Patient(
  patientId: json['patientId'] as String,
  firstName: json['firstName'] as String,
  lastName: json['lastName'] as String,
  phoneNumber: json['phoneNumber'] as String,
  email: json['email'] as String,
  diagnosis: json['diagnosis'] as String,
  therapistName: json['therapistName'] as String?,
  clinicName: json['clinicName'] as String?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  isActive: json['isActive'] as bool? ?? false,
  exercises:
      (json['exercises'] as List<dynamic>?)
          ?.map((e) => ExerciseAssignment.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  appointments:
      (json['appointments'] as List<dynamic>?)
          ?.map((e) => Appointment.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
);

Map<String, dynamic> _$PatientToJson(_Patient instance) => <String, dynamic>{
  'patientId': instance.patientId,
  'firstName': instance.firstName,
  'lastName': instance.lastName,
  'phoneNumber': instance.phoneNumber,
  'email': instance.email,
  'diagnosis': instance.diagnosis,
  'therapistName': instance.therapistName,
  'clinicName': instance.clinicName,
  'createdAt': instance.createdAt?.toIso8601String(),
  'isActive': instance.isActive,
  'exercises': instance.exercises,
  'appointments': instance.appointments,
};
