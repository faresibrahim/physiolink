import 'package:dartz/dartz.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment_slot.dart';

abstract class SlotRepository {
  // Available slots for the caller's assigned therapist (empty if unassigned).
  Future<Either<AppFailure, List<AppointmentSlot>>> getMySlots();

  // Request a slot (consume-on-request). A 409 maps to SlotUnavailableFailure so
  // the UI can say "that slot was just taken" (spec Phase 7).
  Future<Either<AppFailure, void>> requestSlot({
    required String slotId,
    String? type,
    String? notes,
  });
}
