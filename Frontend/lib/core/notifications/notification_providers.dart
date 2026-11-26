import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/network/core_providers.dart';
import 'package:practice/core/notifications/dio_notification_repository.dart';
import 'package:practice/core/notifications/notification_repository.dart';
import 'package:practice/core/notifications/notification_service.dart';

final notificationRepositoryProvider = Provider<NotificationRepository>((ref) {
  final dio = ref.watch(dioClientProvider).dio;
  return DioNotificationRepository(dio);
});

// Sends this device's FCM token to the API for the signed-in patient.
// Best-effort — a patient who declines the OS permission prompt, or whose
// token registration call fails, should still be able to use the app.
// Takes its dependencies rather than a WidgetRef: callers fire-and-forget this
// from pages the router is about to dispose, and a WidgetRef is dead by then.
Future<void> registerDeviceToken(
  NotificationService service,
  NotificationRepository repository,
) async {
  try {
    final granted = await service.requestPermission();
    if (!granted) {
      debugPrint('[push] notification permission not granted');
      return;
    }

    final token = await service.getToken();
    if (token == null) {
      debugPrint('[push] no FCM token available');
      return;
    }

    final result = await repository.registerDeviceToken(token);
    result.fold(
      (failure) => debugPrint('[push] token registration failed: $failure'),
      (_) => debugPrint('[push] device token registered'),
    );
  } catch (ex) {
    debugPrint('[push] device token registration error: $ex');
  }
}
