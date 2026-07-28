import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

const _localeKey = 'app_locale';

/// Overridden in ProviderScope with the loaded instance (see main.dart).
final sharedPreferencesProvider = Provider<SharedPreferences>(
  (ref) => throw UnimplementedError('override in ProviderScope'),
);

class LocaleController extends Notifier<Locale> {
  @override
  Locale build() {
    final code = ref.read(sharedPreferencesProvider).getString(_localeKey);
    return Locale(code ?? 'en');
  }

  Future<void> setLocale(Locale locale) async {
    await ref
        .read(sharedPreferencesProvider)
        .setString(_localeKey, locale.languageCode);
    state = locale;
  }
}

final localeControllerProvider = NotifierProvider<LocaleController, Locale>(
  LocaleController.new,
);
