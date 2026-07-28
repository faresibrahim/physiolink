import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/core/widgets/empty_state.dart';
import 'package:practice/core/widgets/shimmer_card.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';
import 'package:practice/features/exercises/presentation/providers/exercise_fetch_provider.dart';
import 'package:practice/features/exercises/presentation/widgets/exercise_card.dart';
import 'package:practice/l10n/app_localizations.dart';

enum _FilterTab { all, active, completed }

class ExercisesPage extends ConsumerStatefulWidget {
  const ExercisesPage({super.key});

  @override
  ConsumerState<ExercisesPage> createState() => _ExercisesPageState();
}

class _ExercisesPageState extends ConsumerState<ExercisesPage> {
  String _searchQuery = '';
  _FilterTab _filter = _FilterTab.all;

  @override
  Widget build(BuildContext context) {
    final exercisesAsync = ref.watch(exercisesProvider);
    final l10n = AppLocalizations.of(context)!;

    return SafeArea(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // â”€â”€ Header â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          Padding(
            padding: const EdgeInsets.fromLTRB(
              AppSpacing.md,
              AppSpacing.md,
              AppSpacing.md,
              0,
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  l10n.exercises,
                  style: AppTextStyles.title1.copyWith(
                    fontWeight: FontWeight.w800,
                    letterSpacing: -0.5,
                  ),
                ),
                GestureDetector(
                  onTap: () {},
                  child: Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AppColors.surface,
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: AppColors.divider, width: 1),
                    ),
                    child: const Icon(
                      Icons.tune_rounded,
                      size: 18,
                      color: AppColors.textSecondary,
                    ),
                  ),
                ),
              ],
            ),
          ),

          // â”€â”€ Search â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          Padding(
            padding: const EdgeInsets.fromLTRB(
              AppSpacing.md,
              AppSpacing.md,
              AppSpacing.md,
              0,
            ),
            child: TextField(
              decoration: InputDecoration(
                hintText: l10n.searchExercises,
                hintStyle: AppTextStyles.body.copyWith(
                  color: AppColors.textSecondary,
                ),
                prefixIcon: const Icon(
                  Icons.search_rounded,
                  color: AppColors.textSecondary,
                  size: 20,
                ),
              ),
              onChanged: (v) => setState(() => _searchQuery = v.toLowerCase()),
            ),
          ),

          // â”€â”€ Filter chips â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          exercisesAsync.when(
            data: (result) =>
                result.fold((_) => const SizedBox.shrink(), (exercises) {
                  final activeCount = exercises
                      .where((e) => e.feedback == null)
                      .length;
                  final completedCount = exercises
                      .where((e) => e.feedback != null)
                      .length;

                  return Padding(
                    padding: const EdgeInsets.fromLTRB(
                      AppSpacing.md,
                      AppSpacing.sm,
                      AppSpacing.md,
                      0,
                    ),
                    child: Row(
                      children: [
                        _FilterChip(
                          label: l10n.filterAll,
                          count: exercises.length,
                          selected: _filter == _FilterTab.all,
                          onTap: () => setState(() => _filter = _FilterTab.all),
                        ),
                        const SizedBox(width: AppSpacing.sm),
                        _FilterChip(
                          label: l10n.filterActive,
                          count: activeCount,
                          selected: _filter == _FilterTab.active,
                          onTap: () =>
                              setState(() => _filter = _FilterTab.active),
                        ),
                        const SizedBox(width: AppSpacing.sm),
                        _FilterChip(
                          label: l10n.filterCompleted,
                          count: completedCount,
                          selected: _filter == _FilterTab.completed,
                          onTap: () =>
                              setState(() => _filter = _FilterTab.completed),
                        ),
                      ],
                    ),
                  );
                }),
            loading: () => const SizedBox.shrink(),
            error: (_, _) => const SizedBox.shrink(),
          ),

          // â”€â”€ List â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          Expanded(
            child: exercisesAsync.when(
              data: (result) => result.fold(
                (failure) => _buildError(failure),
                (exercises) => _buildList(exercises),
              ),
              loading: () => _buildLoading(),
              error: (_, _) => EmptyState(
                icon: Icons.wifi_off_rounded,
                title: l10n.somethingWentWrong,
                subtitle: l10n.unexpectedError,
                ctaLabel: l10n.retry,
                onCta: () => ref.invalidate(exercisesProvider),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildList(List<ExerciseAssignment> exercises) {
    final l10n = AppLocalizations.of(context)!;
    var filtered = exercises.where((e) {
      return e.exerciseName.toLowerCase().contains(_searchQuery);
    }).toList();

    if (_filter == _FilterTab.active) {
      filtered = filtered.where((e) => e.feedback == null).toList();
    } else if (_filter == _FilterTab.completed) {
      filtered = filtered.where((e) => e.feedback != null).toList();
    }

    if (filtered.isEmpty) {
      return EmptyState(
        icon: _searchQuery.isNotEmpty
            ? Icons.search_off_rounded
            : Icons.fitness_center_outlined,
        title: _searchQuery.isNotEmpty
            ? l10n.noMatchesFor(_searchQuery)
            : _filter == _FilterTab.completed
            ? l10n.noCompletedExercises
            : l10n.noExercisesFound,
        subtitle: _searchQuery.isNotEmpty
            ? l10n.searchNoMatchSubtitle
            : l10n.noExercisesAssignedSubtitle,
        ctaLabel: _searchQuery.isNotEmpty ? l10n.clearSearch : null,
        onCta: _searchQuery.isNotEmpty
            ? () => setState(() => _searchQuery = '')
            : null,
      );
    }

    final isWide = MediaQuery.sizeOf(context).width >= 600;

    return RefreshIndicator(
      color: AppColors.primary,
      onRefresh: () async => ref.invalidate(exercisesProvider),
      child: isWide
          ? GridView.builder(
              padding: const EdgeInsets.fromLTRB(
                AppSpacing.md,
                AppSpacing.md,
                AppSpacing.md,
                AppSpacing.xl,
              ),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                crossAxisSpacing: AppSpacing.md,
                mainAxisSpacing: AppSpacing.md,
                childAspectRatio: 1.5,
              ),
              itemCount: filtered.length,
              itemBuilder: (context, index) {
                final exercise = filtered[index];
                return ExerciseCard(
                  exercise: exercise,
                  onTap: () => context.push('/exercise', extra: exercise),
                );
              },
            )
          : ListView.separated(
              padding: const EdgeInsets.fromLTRB(
                AppSpacing.md,
                AppSpacing.md,
                AppSpacing.md,
                AppSpacing.xl,
              ),
              itemCount: filtered.length,
              separatorBuilder: (_, _) =>
                  const SizedBox(height: AppSpacing.sm),
              itemBuilder: (context, index) {
                final exercise = filtered[index];
                return ExerciseCard(
                  exercise: exercise,
                  onTap: () => context.push('/exercise', extra: exercise),
                );
              },
            ),
    );
  }

  Widget _buildError(AppFailure failure) {
    final l10n = AppLocalizations.of(context)!;
    final message = switch (failure) {
      NetworkFailure() => l10n.noInternet,
      ServerFailure() => l10n.serverError,
      AuthFailure() => l10n.sessionExpired,
    };
    return EmptyState(
      icon: Icons.error_outline_rounded,
      title: l10n.couldntLoadExercises,
      subtitle: message,
      ctaLabel: l10n.retry,
      onCta: () => ref.invalidate(exercisesProvider),
    );
  }

  Widget _buildLoading() {
    return ListView.separated(
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.md,
        AppSpacing.md,
        AppSpacing.md,
        AppSpacing.xl,
      ),
      physics: const NeverScrollableScrollPhysics(),
      itemCount: 6,
      separatorBuilder: (_, _) => const SizedBox(height: AppSpacing.sm),
      itemBuilder: (_, _) => const ShimmerCard(height: 76),
    );
  }
}

class _FilterChip extends StatelessWidget {
  const _FilterChip({
    required this.label,
    required this.count,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final int count;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 150),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 7),
        decoration: BoxDecoration(
          color: selected ? AppColors.primary : AppColors.surface,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: selected ? AppColors.primary : AppColors.divider,
            width: 1,
          ),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              label,
              style: AppTextStyles.footnote.copyWith(
                color: selected ? Colors.white : AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(width: 5),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
              decoration: BoxDecoration(
                color: selected
                    ? Colors.white.withValues(alpha: 0.25)
                    : AppColors.background,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(
                '$count',
                style: AppTextStyles.caption.copyWith(
                  color: selected ? Colors.white : AppColors.textSecondary,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
