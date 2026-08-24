import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/network/core_providers.dart';
import 'package:practice/core/notifications/dio_notification_repository.dart';
import 'package:practice/core/notifications/notification_repository.dart';

final notificationRepositoryProvider = Provider<NotificationRepository>((ref) {
  final dio = ref.watch(dioClientProvider).dio;
  return DioNotificationRepository(dio);
});
