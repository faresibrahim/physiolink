import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';

part 'patient.freezed.dart';
part 'patient.g.dart';

@freezed
abstract class Patient with _$Patient {
  const factory Patient({
    required String patientId,
    required String firstName,
    required String lastName,
    required String phoneNumber,
    required String email,
    required String diagnosis,
    DateTime? createdAt,
    @Default(false) bool isActive,
    @Default([]) List<ExerciseAssignment> exercises,
    @Default([]) List<Appointment> appointments,
  }) = _Patient;

  factory Patient.fromJson(Map<String, dynamic> json) =>
      _$PatientFromJson(json);
}
