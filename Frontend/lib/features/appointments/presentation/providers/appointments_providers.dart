import 'package:dartz/dartz.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/network/core_providers.dart';
import 'package:practice/features/appointments/data/repositories/dio_appointment_repository.dart';
import 'package:practice/features/appointments/data/repositories/dio_slot_repository.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/domain/entities/appointment_slot.dart';
import 'package:practice/features/appointments/domain/repositories/appointments_repository.dart';
import 'package:practice/features/appointments/domain/repositories/slot_repository.dart';

final appointmentsRepositoryProvider = Provider<AppointmentsRepository>((ref) {
  final dio = ref.watch(dioClientProvider).dio;
  return DioAppointmentRepository(dio);
});

final slotRepositoryProvider = Provider<SlotRepository>((ref) {
  final dio = ref.watch(dioClientProvider).dio;
  return DioSlotRepository(dio);
});

// The signed-in patient's appointments (resolved from the JWT, no patientId).
final appointmentsProvider =
    FutureProvider<Either<AppFailure, List<Appointment>>>((ref) async {
      final repository = ref.watch(appointmentsRepositoryProvider);
      return repository.getMyAppointments();
    });

// Available slots for the caller's assigned therapist.
final availableSlotsProvider =
    FutureProvider<Either<AppFailure, List<AppointmentSlot>>>((ref) async {
      final repository = ref.watch(slotRepositoryProvider);
      return repository.getMySlots();
    });

// Drives the "request a slot" action and invalidates the slots + appointments
// providers on success so both lists refresh (spec Phase 7 "Providers").
final slotRequestProvider =
    NotifierProvider<SlotRequestNotifier, AsyncValue<void>>(
      SlotRequestNotifier.new,
    );

class SlotRequestNotifier extends Notifier<AsyncValue<void>> {
  @override
  AsyncValue<void> build() => const AsyncValue.data(null);

  // Returns null on success, or the failure so the caller can show a message.
  Future<AppFailure?> request({
    required String slotId,
    String? type,
    String? notes,
  }) async {
    state = const AsyncValue.loading();
    final repository = ref.read(slotRepositoryProvider);
    final result = await repository.requestSlot(
      slotId: slotId,
      type: type,
      notes: notes,
    );
    return result.fold(
      (failure) {
        state = AsyncValue.error(failure, StackTrace.current);
        return failure;
      },
      (_) {
        state = const AsyncValue.data(null);
        ref.invalidate(availableSlotsProvider);
        ref.invalidate(appointmentsProvider);
        return null;
      },
    );
  }
}
