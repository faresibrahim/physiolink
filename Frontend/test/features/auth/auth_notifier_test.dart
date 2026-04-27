import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:practice/features/auth/presentation/providers/auth_notifier.dart';

class MockDio extends Mock implements Dio {}

class MockFlutterSecureStorage extends Mock implements FlutterSecureStorage {}

// A real JWT with sub=patient-xyz and exp=9999999999 (year 2286 — never expires in tests)
const _fakeAccessToken =
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9'
    '.eyJzdWIiOiJwYXRpZW50LXh5eiIsImV4cCI6OTk5OTk5OTk5OX0'
    '.fake_signature';

const _fakeRefreshToken = 'fake-refresh-token';

void main() {
  late MockDio mockDio;
  late MockFlutterSecureStorage mockStorage;
  late AuthNotifier notifier;

  setUp(() {
    mockDio = MockDio();
    mockStorage = MockFlutterSecureStorage();
    notifier = AuthNotifier(mockStorage, mockDio);
  });

  group('AuthNotifier.login', () {
    test('success sets isLoggedIn to true and extracts patientId from JWT', () async {
      when(
        () => mockDio.post(any(), data: any(named: 'data')),
      ).thenAnswer(
        (_) async => Response(
          statusCode: 200,
          requestOptions: RequestOptions(path: ''),
          data: {
            'accessToken': _fakeAccessToken,
            'refreshToken': _fakeRefreshToken,
          },
        ),
      );

      when(
        () => mockStorage.write(key: any(named: 'key'), value: any(named: 'value')),
      ).thenAnswer((_) async {});

      await notifier.login('patient@example.com', 'secret123');

      expect(notifier.isLoggedIn, true);
      expect(notifier.accessToken, _fakeAccessToken);
      expect(notifier.patientId, 'patient-xyz');
    });

    test('rethrows DioException on login failure', () async {
      when(
        () => mockDio.post(any(), data: any(named: 'data')),
      ).thenThrow(
        DioException(
          requestOptions: RequestOptions(path: ''),
          type: DioExceptionType.badResponse,
          response: Response(
            statusCode: 401,
            requestOptions: RequestOptions(path: ''),
          ),
        ),
      );

      expect(
        () => notifier.login('wrong@example.com', 'badpass'),
        throwsA(isA<DioException>()),
      );

      expect(notifier.isLoggedIn, false);
      expect(notifier.patientId, null);
    });
  });
}
