import 'package:dartz/dartz.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/network/core_providers.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';
import 'package:practice/features/profile/data/repositories/dio_profile_repository.dart';
import 'package:practice/features/profile/domain/entities/patient.dart';
import 'package:practice/features/profile/domain/repositories/profile_repository.dart';

final profileRepositoryProvider = Provider<ProfileRepository>((ref) {
  final dio = ref.watch(dioClientProvider).dio;
  return DioProfileRepository(dio);
});

final profileProvider = FutureProvider<Either<AppFailure, Patient>>((
  ref,
) async {
  final patientId = ref.read(authNotifierProvider).patientId;
  if (patientId == null) return Left(AuthFailure());

  final repository = ref.watch(profileRepositoryProvider);
  return repository.getProfile(patientId);
});
