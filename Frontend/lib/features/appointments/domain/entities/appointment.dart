import 'package:freezed_annotation/freezed_annotation.dart';

part 'appointment.freezed.dart';
part 'appointment.g.dart';

enum AppointmentStatus {
  @JsonValue('Pending') pending,
  @JsonValue('Confirmed') confirmed,
  @JsonValue('Cancelled') cancelled,
  @JsonValue('Completed') completed,
}

@freezed
abstract class Appointment with _$Appointment {
  const factory Appointment({
    required String appointmentId,
    required String title,
    required String patientId,
    required String therapistId,
    DateTime? appointmentTime,
    @Default(AppointmentStatus.pending) AppointmentStatus status,
    DateTime? createdAt,
  }) = _Appointment;

  factory Appointment.fromJson(Map<String, dynamic> json) =>
      _$AppointmentFromJson(json);
}
