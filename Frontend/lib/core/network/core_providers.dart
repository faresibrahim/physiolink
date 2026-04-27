import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:practice/core/network/dio_client.dart';

final dioClientProvider = Provider<DioClient>((ref) => DioClient());
final secureStorageProvider = Provider<FlutterSecureStorage>(
  (ref) => FlutterSecureStorage(),
);
