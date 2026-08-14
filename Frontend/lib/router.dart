import 'package:go_router/go_router.dart';
import 'package:practice/features/appointments/presentation/pages/slot_booking_page.dart';
import 'package:practice/features/appointments/presentation/pages/appointments_page.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';
import 'package:practice/features/profile/presentation/pages/profile_page.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/features/exercises/presentation/pages/exercise_detail_page.dart';
import 'package:practice/features/auth/presentation/pages/login_page.dart';
import 'package:practice/features/auth/presentation/pages/change_password_page.dart';
import 'package:practice/features/auth/presentation/pages/splash_page.dart';
import 'package:practice/features/exercises/presentation/pages/exercises_page.dart';
import 'package:practice/features/homepage/presentation/home_page.dart';
import 'package:practice/features/homepage/presentation/shell_page.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final authNotifier = ref.read(authNotifierProvider);
  return GoRouter(
    initialLocation: '/splash',
    refreshListenable: authNotifier,
    redirect: (context, state) {
      final onSplash = state.matchedLocation == '/splash';
      if (onSplash) return null;
      final onLogin = state.matchedLocation == '/';
      if (!authNotifier.isLoggedIn && !onLogin) return '/';
      if (authNotifier.isLoggedIn && onLogin) return '/home';

      // A patient still on the admin-issued temporary password must set their own
      // before reaching anything else — pin them to the change-password screen.
      final onChangePassword = state.matchedLocation == '/change-password';
      if (authNotifier.isLoggedIn && authNotifier.mustChangePassword) {
        return onChangePassword ? null : '/change-password';
      }
      if (onChangePassword) return '/home';
      return null;
    },
    routes: [
      GoRoute(path: '/splash', builder: (context, state) => const SplashPage()),
      GoRoute(path: '/', builder: (context, state) => const LoginPage()),
      GoRoute(
        path: '/change-password',
        builder: (context, state) => const ChangePasswordPage(),
      ),
      ShellRoute(
        builder: (context, state, child) => ShellPage(child: child),
        routes: [
          GoRoute(path: '/home', builder: (context, state) => const HomePage()),
          GoRoute(
            path: '/exercises',
            builder: (context, state) => const ExercisesPage(),
          ),
          GoRoute(
            path: '/appointments',
            builder: (context, state) => const AppointmentsPage(),
          ),
          GoRoute(
            path: '/profile',
            builder: (context, state) => const ProfilePage(),
          ),
          GoRoute(
            path: '/exercise',
            builder: (context, state) {
              final assignment = state.extra as ExerciseAssignment;
              return ExerciseDetailPage(assignment: assignment);
            },
          ),
          GoRoute(
            path: '/appointment',
            builder: (context, state) => const SlotBookingPage(),
          ),
        ],
      ),
    ],
  );
});
