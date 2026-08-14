import 'package:freezed_annotation/freezed_annotation.dart';

part 'appointment_slot.freezed.dart';
part 'appointment_slot.g.dart';

// A bookable slot exposed to the patient (spec Phase 7). Slim by design — the
// API's PatientSlotDto only carries the id and the start time.
@freezed
abstract class AppointmentSlot with _$AppointmentSlot {
  const factory AppointmentSlot({
    required String slotId,
    required DateTime scheduledAt,
    // False when the slot is already taken (Requested/Booked). Such slots are
    // still shown in the calendar as "Booked" but are not tappable. Defaults to
    // true so older payloads without the field read as available.
    @Default(true) bool isAvailable,
  }) = _AppointmentSlot;

  factory AppointmentSlot.fromJson(Map<String, dynamic> json) =>
      _$AppointmentSlotFromJson(json);
}
