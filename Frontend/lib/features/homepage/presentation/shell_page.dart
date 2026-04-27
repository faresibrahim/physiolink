import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

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
                label: 'Home',
                icon: Semantics(
                  label: 'Home',
                  child: const Icon(Icons.home_outlined),
                ),
                activeIcon: Semantics(
                  label: 'Home',
                  child: Icon(Icons.home_rounded),
                ),
              ),
              BottomNavigationBarItem(
                label: 'Exercises',
                icon: Semantics(
                  label: 'Exercises',
                  child: const Icon(Icons.fitness_center_outlined),
                ),
                activeIcon: Semantics(
                  label: 'Exercises',
                  child: Icon(Icons.fitness_center_rounded),
                ),
              ),
              BottomNavigationBarItem(
                label: 'Appointments',
                icon: Semantics(
                  label: 'Appointments',
                  child: const Icon(Icons.calendar_today_outlined),
                ),
                activeIcon: Semantics(
                  label: 'Appointments',
                  child: Icon(Icons.calendar_today_rounded),
                ),
              ),
              BottomNavigationBarItem(
                label: 'Profile',
                icon: Semantics(
                  label: 'Profile',
                  child: const Icon(Icons.person_outline_rounded),
                ),
                activeIcon: Semantics(
                  label: 'Profile',
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
