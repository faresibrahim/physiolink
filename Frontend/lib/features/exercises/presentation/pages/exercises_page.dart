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

    return SafeArea(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ── Header ─────────────────────────────────────────────────────────
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
                  'Exercises',
                  style: AppTextStyles.title1.copyWith(
                    fontWeight: FontWeight.w800,
                    letterSpacing: -0.5,
                  ),
                ),
                GestureDetector(
                  onTap: () {},
                  child: Container(
                    width: 36,
                    height: 36,
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

          // ── Search ─────────────────────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.fromLTRB(
              AppSpacing.md,
              AppSpacing.md,
              AppSpacing.md,
              0,
            ),
            child: TextField(
              decoration: InputDecoration(
                hintText: 'Search exercises',
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

          // ── Filter chips ───────────────────────────────────────────────────
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
                          label: 'All',
                          count: exercises.length,
                          selected: _filter == _FilterTab.all,
                          onTap: () => setState(() => _filter = _FilterTab.all),
                        ),
                        const SizedBox(width: AppSpacing.sm),
                        _FilterChip(
                          label: 'Active',
                          count: activeCount,
                          selected: _filter == _FilterTab.active,
                          onTap: () =>
                              setState(() => _filter = _FilterTab.active),
                        ),
                        const SizedBox(width: AppSpacing.sm),
                        _FilterChip(
                          label: 'Completed',
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
            error: (_, __) => const SizedBox.shrink(),
          ),

          // ── List ───────────────────────────────────────────────────────────
          Expanded(
            child: exercisesAsync.when(
              data: (result) => result.fold(
                (failure) => _buildError(failure),
                (exercises) => _buildList(exercises),
              ),
              loading: () => _buildLoading(),
              error: (_, __) => EmptyState(
                icon: Icons.wifi_off_rounded,
                title: 'Something went wrong',
                subtitle: 'An unexpected error occurred.',
                ctaLabel: 'Retry',
                onCta: () => ref.invalidate(exercisesProvider),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildList(List<ExerciseAssignment> exercises) {
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
            ? 'No matches for "$_searchQuery"'
            : _filter == _FilterTab.completed
            ? 'No completed exercises'
            : 'No exercises found',
        subtitle: _searchQuery.isNotEmpty
            ? 'Try a different keyword, or browse your full exercise plan.'
            : "Your therapist hasn't assigned any exercises yet.",
        ctaLabel: _searchQuery.isNotEmpty ? 'Clear search' : null,
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
              separatorBuilder: (_, __) =>
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
    final message = switch (failure) {
      NetworkFailure() => 'No internet connection',
      ServerFailure() => 'Server error. Try again.',
      AuthFailure() => 'Session expired. Please log in again.',
    };
    return EmptyState(
      icon: Icons.error_outline_rounded,
      title: "Couldn't load exercises",
      subtitle: message,
      ctaLabel: 'Retry',
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
      separatorBuilder: (_, __) => const SizedBox(height: AppSpacing.sm),
      itemBuilder: (_, __) => const ShimmerCard(height: 76),
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
                    ? Colors.white.withOpacity(0.25)
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
