import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:practice/l10n/app_localizations.dart';

class ShellPage extends StatelessWidget {
  const ShellPage({super.key, required this.child});

  final Widget child;

  int _selectedIndex(String location) {
    if (location.startsWith('/exercises')) return 1;
    if (location.startsWith('/appointments')) return 2;
    if (location.startsWith('/profile')) return 3;
    return 0;
  }

  @override
  Widget build(BuildContext context) {
    final location = GoRouterState.of(context).matchedLocation;
    final selectedIndex = _selectedIndex(location);
    final l10n = AppLocalizations.of(context)!;

    return Scaffold(
      body: child,
      bottomNavigationBar: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Divider(height: 1, thickness: 1),
          BottomNavigationBar(
            currentIndex: selectedIndex,
            onTap: (index) {
              const paths = [
                '/home',
                '/exercises',
                '/appointments',
                '/profile',
              ];
              context.go(paths[index]);
            },
            items: [
              BottomNavigationBarItem(
                label: l10n.home,
                icon: Semantics(
                  label: l10n.home,
                  child: const Icon(Icons.home_outlined),
                ),
                activeIcon: Semantics(
                  label: l10n.home,
                  child: Icon(Icons.home_rounded),
                ),
              ),
              BottomNavigationBarItem(
                label: l10n.exercises,
                icon: Semantics(
                  label: l10n.exercises,
                  child: const Icon(Icons.fitness_center_outlined),
                ),
                activeIcon: Semantics(
                  label: l10n.exercises,
                  child: Icon(Icons.fitness_center_rounded),
                ),
              ),
              BottomNavigationBarItem(
                label: l10n.appointments,
                icon: Semantics(
                  label: l10n.appointments,
                  child: const Icon(Icons.calendar_today_outlined),
                ),
                activeIcon: Semantics(
                  label: l10n.appointments,
                  child: Icon(Icons.calendar_today_rounded),
                ),
              ),
              BottomNavigationBarItem(
                label: l10n.profile,
                icon: Semantics(
                  label: l10n.profile,
                  child: const Icon(Icons.person_outline_rounded),
                ),
                activeIcon: Semantics(
                  label: l10n.profile,
                  child: Icon(Icons.person_rounded),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
