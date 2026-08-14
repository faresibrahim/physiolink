import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/appointments/domain/entities/appointment_slot.dart';
import 'package:practice/features/appointments/domain/repositories/slot_repository.dart';

class DioSlotRepository implements SlotRepository {
  final Dio _dio;
  DioSlotRepository(this._dio);

  @override
  Future<Either<AppFailure, List<AppointmentSlot>>> getMySlots() async {
    try {
      final response = await _dio.get('/api/v1/patients/me/slots');
      final slots = (response.data as List)
          .map((item) => AppointmentSlot.fromJson(item as Map<String, dynamic>))
          .toList();
      return Right(slots);
    } catch (ex) {
      return Left(_mapError(ex));
    }
  }

  @override
  Future<Either<AppFailure, void>> requestSlot({
    required String slotId,
    String? type,
    String? notes,
  }) async {
    try {
      await _dio.post(
        '/api/v1/patients/me/appointments',
        data: {
          'slotId': slotId,
          if (type != null && type.isNotEmpty) 'type': type,
          if (notes != null && notes.isNotEmpty) 'notes': notes,
        },
      );
      return const Right(null);
    } catch (ex) {
      return Left(_mapError(ex));
    }
  }

  AppFailure _mapError(Object ex) {
    if (ex is DioException) {
      if (ex.type == DioExceptionType.connectionError) {
        return const NetworkFailure();
      }
      final code = ex.response?.statusCode;
      if (code == 401) return const AuthFailure();
      // Lost the race / slot gone / not their therapist / in the past.
      if (code == 409) return const SlotUnavailableFailure();
      return ServerFailure(code ?? 500);
    }
    return const ServerFailure(500);
  }
}
