import 'package:flutter/material.dart';
import 'package:practice/core/widgets/status_badge.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';

class DifficultyBadge extends StatelessWidget {
  const DifficultyBadge({super.key, required this.difficulty});

  final DifficultyLevel difficulty;

  static Color _colorFor(DifficultyLevel level) => switch (level) {
    DifficultyLevel.easy => AppColors.secondary,
    DifficultyLevel.moderate => AppColors.warning,
    DifficultyLevel.hard => AppColors.destructive,
  };

  static String _labelFor(DifficultyLevel level) => switch (level) {
    DifficultyLevel.easy => 'Easy',
    DifficultyLevel.moderate => 'Moderate',
    DifficultyLevel.hard => 'Hard',
  };

  @override
  Widget build(BuildContext context) {
    return StatusBadge(
      label: _labelFor(difficulty),
      color: _colorFor(difficulty),
    );
  }
}
