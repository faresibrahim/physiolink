import 'dart:convert';

import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

const _androidChannel = AndroidNotificationChannel(
  'physiolink_default',
  'PhysioLink Notifications',
  description: 'Appointment and exercise updates',
  importance: Importance.high,
);

class NotificationService {
  NotificationService(this._messaging, this._localNotifications);
  final FirebaseMessaging _messaging;
  final FlutterLocalNotificationsPlugin _localNotifications;

  Future<bool> requestPermission() async {
    final settings = await _messaging.requestPermission(
      alert: true,
      badge: true,
      sound: true,
    );
    return settings.authorizationStatus == AuthorizationStatus.authorized ||
        settings.authorizationStatus == AuthorizationStatus.provisional;
  }

  Future<String?> getToken() => _messaging.getToken();

  Stream<String> get onTokenRefresh => _messaging.onTokenRefresh;

  // Background/terminated: the OS displays the `notification` block on its own —
  // nothing to wire up here. Foreground is the one state both platforms suppress
  // the system banner in, so we show it ourselves via local notifications, using
  // the same `notification` block the OS would have rendered.
  Future<void> initialize() async {
    await _localNotifications
        .resolvePlatformSpecificImplementation<
            AndroidFlutterLocalNotificationsPlugin>()
        ?.createNotificationChannel(_androidChannel);

    await _localNotifications.initialize(
      settings: const InitializationSettings(
        android: AndroidInitializationSettings('@mipmap/ic_launcher'),
        iOS: DarwinInitializationSettings(),
      ),
    );

    FirebaseMessaging.onMessage.listen((message) {
      final notification = message.notification;
      if (notification == null) return;
      _localNotifications.show(
        id: message.hashCode,
        title: notification.title,
        body: notification.body,
        notificationDetails: NotificationDetails(
          android: AndroidNotificationDetails(
            _androidChannel.id,
            _androidChannel.name,
          ),
          iOS: const DarwinNotificationDetails(),
        ),
        payload: jsonEncode(message.data),
      );
    });
  }
}

final notificationServiceProvider = Provider<NotificationService>(
  (ref) => NotificationService(
    FirebaseMessaging.instance,
    FlutterLocalNotificationsPlugin(),
  ),
);
