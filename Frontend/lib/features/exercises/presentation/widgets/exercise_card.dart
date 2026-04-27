import 'package:flutter/material.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';

class ExerciseCard extends StatelessWidget {
  const ExerciseCard({super.key, required this.exercise, required this.onTap});

  final ExerciseAssignment exercise;
  final VoidCallback onTap;

  Color get _difficultyColor => switch (exercise.difficulty) {
        DifficultyLevel.easy => AppColors.secondary,
        DifficultyLevel.moderate => const Color(0xFFF59E0B),
        DifficultyLevel.hard => AppColors.destructive,
      };

  Color get _difficultyBg => switch (exercise.difficulty) {
        DifficultyLevel.easy => AppColors.secondaryLight,
        DifficultyLevel.moderate => const Color(0xFFFFF8E6),
        DifficultyLevel.hard => const Color(0xFFFFEEEE),
      };

  bool get _isCompleted => exercise.feedback != null;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.md),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Row(
          children: [
            // Circle icon
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: _isCompleted
                    ? AppColors.secondaryLight
                    : AppColors.primaryLight,
                shape: BoxShape.circle,
              ),
              child: Icon(
                _isCompleted
                    ? Icons.check_rounded
                    : Icons.play_arrow_rounded,
                size: 22,
                color: _isCompleted ? AppColors.secondary : AppColors.primary,
              ),
            ),

            const SizedBox(width: AppSpacing.md),

            // Content
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          exercise.exerciseName,
                          style: AppTextStyles.subheadline.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 3,
                        ),
                        decoration: BoxDecoration(
                          color: _difficultyBg,
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(
                          exercise.difficulty.name[0].toUpperCase() +
                              exercise.difficulty.name.substring(1),
                          style: AppTextStyles.caption.copyWith(
                            color: _difficultyColor,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 5),
                  Row(
                    children: [
                      _MiniStat(
                        icon: Icons.refresh_rounded,
                        label: '${exercise.sets} sets',
                      ),
                      const SizedBox(width: 10),
                      _MiniStat(
                        icon: Icons.repeat_rounded,
                        label: '${exercise.reps} reps',
                      ),
                      const SizedBox(width: 10),
                      _MiniStat(
                        icon: Icons.timer_outlined,
                        label: '${exercise.durationMinutes} min',
                      ),
                    ],
                  ),
                ],
              ),
            ),

            const SizedBox(width: AppSpacing.sm),
            const Icon(
              Icons.chevron_right_rounded,
              color: AppColors.textSecondary,
              size: 20,
            ),
          ],
        ),
      ),
    );
  }
}

class _MiniStat extends StatelessWidget {
  const _MiniStat({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 12, color: AppColors.textSecondary),
        const SizedBox(width: 3),
        Text(
          label,
          style: AppTextStyles.caption.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
      ],
    );
  }
}
