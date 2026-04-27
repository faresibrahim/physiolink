import 'package:flutter_riverpod/legacy.dart';
import 'package:practice/core/network/core_providers.dart';
import 'package:practice/features/auth/presentation/providers/auth_notifier.dart';

final authNotifierProvider = ChangeNotifierProvider<AuthNotifier>((ref) {
  final secureStorage = ref.watch(secureStorageProvider);
  final dioClient = ref.watch(dioClientProvider);
  final notifier = AuthNotifier(secureStorage, dioClient.dio);
  dioClient.tokenGetter = () => notifier.accessToken;
  dioClient.refreshTokenGetter = () => notifier.refreshToken;
  dioClient.tokenUpdater = (accessToken, refreshToken) =>
      notifier.updateTokens(accessToken, refreshToken);
  return notifier;
});
