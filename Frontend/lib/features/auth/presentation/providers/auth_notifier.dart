import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthNotifier extends ChangeNotifier {
  static const _emptyGuid = '00000000-0000-0000-0000-000000000000';

  String? _accessToken;
  String? _refreshToken;
  String? _patientId;
  bool _mustChangePassword = false;
  final FlutterSecureStorage _secureStorage;
  final Dio _dio;

  bool get isLoggedIn => accessToken != null;
  String? get refreshToken => _refreshToken;

  String? get accessToken => _accessToken;
  String? get patientId => _patientId;

  // True while the patient is still on the admin-issued temporary password.
  // The router uses this to force the change-password screen before anything else.
  bool get mustChangePassword => _mustChangePassword;

  bool _isInvalidPatientId(String? id) =>
      id == null || id.isEmpty || id == _emptyGuid;

  AuthNotifier(
    this._secureStorage,
    this._dio,
  ); //pass the secure storage through parameter

  Future<void> tryRestoreSession() async {
    final storedAccess = await _secureStorage.read(key: 'access_token');
    final storedRefresh = await _secureStorage.read(key: 'refresh_token');
    final storedPatientId = await _secureStorage.read(key: 'patient_id');
    final storedMustChange =
        await _secureStorage.read(key: 'must_change_password');

    if (storedAccess == null) return; // Nothing stored → go to login

    // Legacy / broken session: patient_id missing or Guid.Empty.
    // Clear everything and force a fresh login so the new flow runs.
    if (_isInvalidPatientId(storedPatientId)) {
      await _secureStorage.delete(key: 'access_token');
      await _secureStorage.delete(key: 'refresh_token');
      await _secureStorage.delete(key: 'patient_id');
      return;
    }

    // Access token still valid → restore immediately, no network call needed
    if (_isTokenValid(storedAccess)) {
      _accessToken = storedAccess;
      _refreshToken = storedRefresh;
      _patientId = storedPatientId;
      _mustChangePassword = storedMustChange == 'true';
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
      final newPatientId = response.data['patientId'] as String;
      final mustChange = response.data['mustChangePassword'] as bool? ?? false;

      await _secureStorage.write(key: 'access_token', value: newAccess);
      await _secureStorage.write(key: 'refresh_token', value: newRefresh);
      await _secureStorage.write(key: 'patient_id', value: newPatientId);
      await _secureStorage.write(
        key: 'must_change_password',
        value: mustChange.toString(),
      );

      _accessToken = newAccess;
      _refreshToken = newRefresh;
      _patientId = newPatientId;
      _mustChangePassword = mustChange;
      notifyListeners();
    } catch (_) {
      // Refresh failed (token revoked or server error) → clear storage,
      // do nothing, user will be sent to login by the router
      await _secureStorage.delete(key: 'access_token');
      await _secureStorage.delete(key: 'refresh_token');
      await _secureStorage.delete(key: 'patient_id');
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

  Future<void> login(String username, String password) async {
    try {
      var response = await _dio.post(
        ('/api/v1/auth/login'),
        data: {'username': username, 'password': password},
      );

      final receivedPatientId = response.data['patientId'] as String?;
      if (_isInvalidPatientId(receivedPatientId)) {
        throw StateError(
          'Login response missing a valid patientId — backend may be out of date.',
        );
      }

      _accessToken = response.data['accessToken'] as String;
      _refreshToken = response.data['refreshToken'] as String;
      _patientId = receivedPatientId;
      _mustChangePassword =
          response.data['mustChangePassword'] as bool? ?? false;

      await _secureStorage.write(key: 'access_token', value: _accessToken);
      await _secureStorage.write(key: 'refresh_token', value: _refreshToken);
      await _secureStorage.write(key: 'patient_id', value: _patientId);
      await _secureStorage.write(
        key: 'must_change_password',
        value: _mustChangePassword.toString(),
      );

      notifyListeners();
    } catch (ex) {
      rethrow;
    }
  }

  // Replaces the temporary password with one the patient chooses. The patient
  // already authenticated with the temporary password at login, so it isn't asked
  // for again here. The backend rotates tokens and clears the must-change flag;
  // we persist the fresh session so the router lets the patient through.
  Future<void> changePassword(String newPassword) async {
    final response = await _dio.post(
      '/api/v1/auth/change-password',
      data: {
        'newPassword': newPassword,
      },
    );

    _accessToken = response.data['accessToken'] as String;
    _refreshToken = response.data['refreshToken'] as String;
    _mustChangePassword =
        response.data['mustChangePassword'] as bool? ?? false;

    await _secureStorage.write(key: 'access_token', value: _accessToken);
    await _secureStorage.write(key: 'refresh_token', value: _refreshToken);
    await _secureStorage.write(
      key: 'must_change_password',
      value: _mustChangePassword.toString(),
    );

    notifyListeners();
  }

  Future<void> logout() async {
    await _secureStorage.delete(key: 'access_token');
    await _secureStorage.delete(key: 'refresh_token');
    await _secureStorage.delete(key: 'patient_id');
    await _secureStorage.delete(key: 'must_change_password');
    _accessToken = null;
    _refreshToken = null;
    _patientId = null;
    _mustChangePassword = false;
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
