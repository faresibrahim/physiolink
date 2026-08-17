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
  final _searchController = TextEditingController();
  String _searchQuery = '';
  _FilterTab _filter = _FilterTab.all;

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  void _clearSearch() {
    _searchController.clear();
    setState(() => _searchQuery = '');
  }

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
            child: Text(
              l10n.exercises,
              style: AppTextStyles.title1.copyWith(
                fontWeight: FontWeight.w800,
                letterSpacing: -0.5,
              ),
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
              controller: _searchController,
              textInputAction: TextInputAction.search,
              style: AppTextStyles.body,
              decoration: InputDecoration(
                filled: true,
                fillColor: AppColors.surface,
                contentPadding: const EdgeInsets.symmetric(
                  vertical: AppSpacing.sm,
                ),
                hintText: l10n.searchExercises,
                hintStyle: AppTextStyles.body.copyWith(
                  color: AppColors.textSecondary,
                ),
                prefixIcon: const Icon(
                  Icons.search_rounded,
                  color: AppColors.textSecondary,
                  size: 20,
                ),
                suffixIcon: AnimatedSwitcher(
                  duration: const Duration(milliseconds: 150),
                  transitionBuilder: (child, anim) =>
                      FadeTransition(opacity: anim, child: child),
                  child: _searchQuery.isEmpty
                      ? const SizedBox.shrink()
                      : Semantics(
                          button: true,
                          label: l10n.clearSearch,
                          child: GestureDetector(
                            onTap: _clearSearch,
                            child: const Icon(
                              Icons.cancel_rounded,
                              color: AppColors.textSecondary,
                              size: 18,
                            ),
                          ),
                        ),
                ),
                suffixIconConstraints: const BoxConstraints(
                  minWidth: 44,
                  minHeight: 44,
                ),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(AppRadius.pill),
                  borderSide: BorderSide.none,
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(AppRadius.pill),
                  borderSide: BorderSide.none,
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(AppRadius.pill),
                  borderSide: const BorderSide(
                    color: AppColors.primary,
                    width: 1.5,
                  ),
                ),
              ),
              onChanged: (v) => setState(() => _searchQuery = v.toLowerCase()),
            ),
          ),

          // â”€â”€ Filter â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          Padding(
            padding: const EdgeInsets.fromLTRB(
              AppSpacing.md,
              AppSpacing.md,
              AppSpacing.md,
              0,
            ),
            child: _FilterSegments(
              selected: _filter,
              labels: [l10n.filterAll, l10n.filterActive, l10n.filterCompleted],
              onChanged: (tab) => setState(() => _filter = tab),
            ),
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
        onCta: _searchQuery.isNotEmpty ? _clearSearch : null,
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

/// Segmented control with a single pill that slides between the options,
/// so switching filters reads as one continuous movement.
class _FilterSegments extends StatelessWidget {
  const _FilterSegments({
    required this.selected,
    required this.labels,
    required this.onChanged,
  });

  final _FilterTab selected;
  final List<String> labels;
  final ValueChanged<_FilterTab> onChanged;

  @override
  Widget build(BuildContext context) {
    const tabs = _FilterTab.values;
    final index = tabs.indexOf(selected);

    return Container(
      height: 44,
      padding: const EdgeInsets.all(AppSpacing.xs),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(AppRadius.pill),
      ),
      child: Stack(
        children: [
          // Sliding indicator. Alignment runs -1 → 1 across the three slots.
          AnimatedAlign(
            duration: const Duration(milliseconds: 240),
            curve: Curves.easeOutCubic,
            alignment: AlignmentDirectional(index - 1.0, 0),
            child: FractionallySizedBox(
              widthFactor: 1 / tabs.length,
              heightFactor: 1,
              child: Container(
                decoration: BoxDecoration(
                  color: AppColors.primary,
                  borderRadius: BorderRadius.circular(AppRadius.pill),
                ),
              ),
            ),
          ),
          Row(
            children: [
              for (var i = 0; i < tabs.length; i++)
                Expanded(
                  child: Semantics(
                    button: true,
                    selected: i == index,
                    child: GestureDetector(
                      behavior: HitTestBehavior.opaque,
                      onTap: () => onChanged(tabs[i]),
                      child: Center(
                        child: AnimatedDefaultTextStyle(
                          duration: const Duration(milliseconds: 240),
                          curve: Curves.easeOutCubic,
                          style: AppTextStyles.footnote.copyWith(
                            color: i == index
                                ? Colors.white
                                : AppColors.textSecondary,
                            fontWeight: i == index
                                ? FontWeight.w700
                                : FontWeight.w500,
                          ),
                          child: Text(labels[i]),
                        ),
                      ),
                    ),
                  ),
                ),
            ],
          ),
        ],
      ),
    );
  }
}
