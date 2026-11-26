import 'dart:convert';

import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
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

  // On iOS the FCM token is derived from the APNs token, which Apple hands
  // over asynchronously after the permission prompt. Asking for the FCM token
  // before that throws `apns-token-not-set`, so wait for the APNs token first.
  Future<String?> getToken() async {
    if (defaultTargetPlatform == TargetPlatform.iOS) {
      final apnsToken = await _waitForApnsToken();
      if (apnsToken == null) {
        debugPrint('[push] APNs token never arrived; skipping FCM token');
        return null;
      }
    }
    return _messaging.getToken();
  }

  Future<String?> _waitForApnsToken() async {
    const attempts = 15;
    for (var i = 0; i < attempts; i++) {
      final token = await _messaging.getAPNSToken();
      if (token != null) return token;
      await Future<void>.delayed(const Duration(seconds: 2));
    }
    return null;
  }

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
