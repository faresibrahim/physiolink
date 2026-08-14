import 'package:freezed_annotation/freezed_annotation.dart';

part 'appointment.freezed.dart';
part 'appointment.g.dart';

// Widened to match the API's expanded AppointmentStatus (spec 1.2). @JsonValue
// strings match the API's serialized enum names.
enum AppointmentStatus {
  @JsonValue('Requested') requested,
  @JsonValue('Confirmed') confirmed,
  @JsonValue('Completed') completed,
  @JsonValue('Rejected') rejected,
  @JsonValue('Expired') expired,
  @JsonValue('CancelledByClinic') cancelledByClinic,
}

@freezed
abstract class Appointment with _$Appointment {
  const factory Appointment({
    required String appointmentId,
    @Default('') String title,
    String? therapistName,
    String? notes,
    DateTime? appointmentTime,
    @Default(AppointmentStatus.requested) AppointmentStatus status,
    // Honest, server-provided display wording (e.g. "Pending — awaiting
    // confirmation"). Kept alongside `status` so the UI never has to imply a
    // pending request is confirmed (spec Phase 7).
    @Default('') String statusLabel,
    String? slotId,
  }) = _Appointment;

  factory Appointment.fromJson(Map<String, dynamic> json) =>
      _$AppointmentFromJson(json);
}
