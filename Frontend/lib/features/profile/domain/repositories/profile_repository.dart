import 'package:dartz/dartz.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/features/profile/domain/entities/patient.dart';

abstract class ProfileRepository {
  Future<Either<AppFailure, Patient>> getProfile(String patientId);

  Future<Either<AppFailure, void>> updateProfile({
    required String patientId,
    required String firstName,
    required String lastName,
    required String phoneNumber,
  });
}
