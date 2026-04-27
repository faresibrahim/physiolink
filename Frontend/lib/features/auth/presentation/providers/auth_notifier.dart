import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthNotifier extends ChangeNotifier {
  String? _accessToken;
  String? _refreshToken;
  String? _patientId;
  final FlutterSecureStorage _secureStorage;
  final Dio _dio;

  bool get isLoggedIn => accessToken != null;
  String? get refreshToken => _refreshToken;

  String? get accessToken => _accessToken;
  String? get patientId => _patientId;

  AuthNotifier(
    this._secureStorage,
    this._dio,
  ); //pass the secure storage through parameter

  Future<void> tryRestoreSession() async {
    final storedAccess = await _secureStorage.read(key: 'access_token');
    final storedRefresh = await _secureStorage.read(key: 'refresh_token');

    if (storedAccess == null) return; // Nothing stored → go to login

    // Access token still valid → restore immediately, no network call needed
    if (_isTokenValid(storedAccess)) {
      final claims = _decodeJwt(storedAccess);
      _accessToken = storedAccess;
      _refreshToken = storedRefresh;
      _patientId = claims['sub'] as String;
      notifyListeners();
      return;
    }

    // Access token expired → try refresh token silently
    if (storedRefresh == null) return;

    try {
      final response = await _dio.post(
        '/api/v1/auth/refresh',
        data: {'refreshToken': storedRefresh},
      );
      final newAccess = response.data['accessToken'] as String;
      final newRefresh = response.data['refreshToken'] as String;

      await _secureStorage.write(key: 'access_token', value: newAccess);
      await _secureStorage.write(key: 'refresh_token', value: newRefresh);

      final claims = _decodeJwt(newAccess);
      _accessToken = newAccess;
      _refreshToken = newRefresh;
      _patientId = claims['sub'] as String;
      notifyListeners();
    } catch (_) {
      // Refresh failed (token revoked or server error) → clear storage,
      // do nothing, user will be sent to login by the router
      await _secureStorage.delete(key: 'access_token');
      await _secureStorage.delete(key: 'refresh_token');
    }
  }

  bool _isTokenValid(String token) {
    try {
      final claims = _decodeJwt(token);
      final exp = claims['exp'] as int;
      final expiry = DateTime.fromMillisecondsSinceEpoch(exp * 1000, isUtc: true);
      // Treat as expired 30 seconds early to avoid edge-case failures
      return DateTime.now().toUtc().isBefore(
            expiry.subtract(const Duration(seconds: 30)),
          );
    } catch (_) {
      return false;
    }
  }

  Future<void> login(String email, String password) async {
    try {
      var response = await _dio.post(
        ('/api/v1/auth/login'),
        data: {'email': email, 'password': password},
      );

      _accessToken = response.data['accessToken'] as String;
      _refreshToken = response.data['refreshToken'] as String;
      final claims = _decodeJwt(_accessToken!);
      await _secureStorage.write(key: 'access_token', value: accessToken);
      await _secureStorage.write(key: 'refresh_token', value: refreshToken);

      _patientId = claims['sub'] as String;
      notifyListeners();
    } catch (ex) {
      rethrow;
    }
  }

  void logout() {
    _accessToken = null;
    _refreshToken = null;
    _patientId = null;
    notifyListeners();
  }

  Future<void> updateTokens(String accessToken, String refreshToken) async {
    await _secureStorage.write(key: 'access_token', value: accessToken);
    await _secureStorage.write(key: 'refresh_token', value: refreshToken);
    _accessToken = accessToken;
    _refreshToken = refreshToken;
  }

  Map<String, dynamic> _decodeJwt(String token) {
    final parts = token.split('.');
    final payload = parts[1];
    // Base64 needs padding to be a multiple of 4
    final normalized = base64Url.normalize(payload);
    final decoded = utf8.decode(base64Url.decode(normalized));
    return jsonDecode(decoded);
  }
}
