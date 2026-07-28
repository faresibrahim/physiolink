import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/core/widgets/empty_state.dart';
import 'package:practice/core/widgets/shimmer_card.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/presentation/providers/appointments_providers.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';
import 'package:practice/features/exercises/presentation/providers/exercise_fetch_provider.dart';
import 'package:practice/features/exercises/presentation/widgets/exercise_card.dart';
import 'package:practice/features/profile/presentation/providers/profile_providers.dart';
import 'package:practice/l10n/app_localizations.dart';


class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  String _greeting(AppLocalizations l10n) {
    final hour = DateTime.now().hour;
    if (hour < 12) return l10n.goodMorning;
    if (hour < 17) return l10n.goodAfternoon;
    return l10n.goodEvening;
  }

  static const _weekdayAbbr = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  // Returns the date-only part (midnight) of a DateTime in local time
  static DateTime _dateOnly(DateTime dt) {
    final local = dt.toLocal();
    return DateTime(local.year, local.month, local.day);
  }

  // Number of exercises completed this week (Monâ€“now)
  static int _weeklyCompleted(List<ExerciseAssignment> exercises) {
    final now = DateTime.now();
    final monday = _dateOnly(now).subtract(Duration(days: now.weekday - 1));
    return exercises
        .where((e) =>
            e.completedAt != null &&
            !_dateOnly(e.completedAt!).isBefore(monday))
        .length;
  }

  // Set of weekday numbers (1=Mon â€¦ 7=Sun) where at least one exercise
  // was completed this week â€” used to light up the day circles
  static Set<int> _weekDaysDone(List<ExerciseAssignment> exercises) {
    final now = DateTime.now();
    final monday = _dateOnly(now).subtract(Duration(days: now.weekday - 1));
    final sunday = monday.add(const Duration(days: 6));
    return exercises
        .where((e) {
          if (e.completedAt == null) return false;
          final d = _dateOnly(e.completedAt!);
          return !d.isBefore(monday) && !d.isAfter(sunday);
        })
        .map((e) => _dateOnly(e.completedAt!).weekday)
        .toSet();
  }

  // Consecutive-day streak ending today (or yesterday)
  static int _calculateStreak(List<ExerciseAssignment> exercises) {
    final completedDays = exercises
        .where((e) => e.completedAt != null)
        .map((e) => _dateOnly(e.completedAt!))
        .toSet()
        .toList()
      ..sort((a, b) => b.compareTo(a));

    if (completedDays.isEmpty) return 0;

    final today = _dateOnly(DateTime.now());
    final yesterday = today.subtract(const Duration(days: 1));

    // Streak must include today or yesterday to be active
    if (completedDays.first != today && completedDays.first != yesterday) {
      return 0;
    }

    var streak = 0;
    var expected = completedDays.first;
    for (final day in completedDays) {
      if (day == expected) {
        streak++;
        expected = expected.subtract(const Duration(days: 1));
      } else {
        break;
      }
    }
    return streak;
  }

  String _dayAbbr(DateTime dt) => _weekdayAbbr[dt.weekday - 1];

  String _timeLabel(DateTime dt) {
    final h = dt.hour % 12 == 0 ? 12 : dt.hour % 12;
    final m = dt.minute.toString().padLeft(2, '0');
    final p = dt.hour >= 12 ? 'pm' : 'am';
    return '$h:$m $p';
  }

  String _formattedDate() {
    const weekdays = [
      'Monday', 'Tuesday', 'Wednesday', 'Thursday',
      'Friday', 'Saturday', 'Sunday',
    ];
    const months = [
      'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
      'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
    ];
    final now = DateTime.now();
    return '${weekdays[now.weekday - 1]}, ${months[now.month - 1]} ${now.day}';
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final exercisesAsync = ref.watch(exercisesProvider);
    final profileAsync = ref.watch(profileProvider);
    final appointmentsAsync = ref.watch(appointmentsProvider);

    // Derive exercise stats from live data
    final exercises = exercisesAsync.when(
      data: (r) => r.fold((_) => <ExerciseAssignment>[], (list) => list),
      loading: () => <ExerciseAssignment>[],
      error: (_, _) => <ExerciseAssignment>[],
    );
    final streak = _calculateStreak(exercises);
    final weeklyCompleted = _weeklyCompleted(exercises);
    final weeklyTotal = exercises.length;
    final weekDaysDone = _weekDaysDone(exercises);

    // Derive the single closest upcoming non-cancelled appointment
    final nextAppointment = appointmentsAsync.when(
      data: (result) => result.fold(
        (_) => null,
        (list) {
          final now = DateTime.now();
          final upcoming = list
              .where((a) =>
                  a.appointmentTime != null &&
                  a.appointmentTime!.isAfter(now) &&
                  a.status != AppointmentStatus.cancelled)
              .toList()
            ..sort((a, b) =>
                a.appointmentTime!.compareTo(b.appointmentTime!));
          return upcoming.isEmpty ? null : upcoming.first;
        },
      ),
      loading: () => null,
      error: (_, _) => null,
    );

    final firstName = profileAsync.when(
      data: (result) => result.fold((_) => null, (p) => p.firstName),
      loading: () => null,
      error: (_, _) => null,
    );

    return SafeArea(
      child: RefreshIndicator(
        color: AppColors.primary,
        onRefresh: () async {
          ref.invalidate(exercisesProvider);
          ref.invalidate(profileProvider);
          ref.invalidate(appointmentsProvider);
        },
        child: CustomScrollView(
          slivers: [
            // â”€â”€ Hero Header â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            SliverToBoxAdapter(
              child: _HeroHeader(
                greeting: firstName != null
                    ? l10n.greetingWithName(_greeting(l10n), firstName)
                    : _greeting(l10n),
                date: _formattedDate(),
              ),
            ),

            // â”€â”€ Stat cards â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(
                  AppSpacing.md,
                  AppSpacing.md,
                  AppSpacing.md,
                  0,
                ),
                child: Row(
                  children: [
                    Expanded(
                      child: _StatCard(
                        icon: Icons.fitness_center_rounded,
                        iconColor: const Color(0xFF7C5CFC),
                        iconBg: const Color(0xFFF0ECFF),
                        value: exercisesAsync.when(
                          data: (r) => r.fold(
                            (_) => 'â€”',
                            (list) {
                              final done =
                                  list.where((e) => e.feedback != null).length;
                              return '$done / ${list.length}';
                            },
                          ),
                          loading: () => '…',
                          error: (_, _) => '—',
                        ),
                        label: l10n.exercisesTodayStat,
                      ),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    Expanded(
                      child: _StatCard(
                        icon: Icons.local_fire_department_rounded,
                        iconColor: const Color(0xFFF59E0B),
                        iconBg: const Color(0xFFFFF8E6),
                        value: '$streak',
                        label: l10n.streakDaysStat,
                      ),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    Expanded(
                      child: _StatCard(
                        icon: Icons.calendar_today_rounded,
                        iconColor: AppColors.secondary,
                        iconBg: AppColors.secondaryLight,
                        value: nextAppointment != null
                            ? _dayAbbr(nextAppointment.appointmentTime!)
                            : '—',
                        label: nextAppointment != null
                            ? l10n.nextTimeStat(
                                _timeLabel(nextAppointment.appointmentTime!),
                              )
                            : l10n.nextNoneStat,
                      ),
                    ),
                  ],
                ),
              ),
            ),

            // â”€â”€ Weekly progress â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(
                  AppSpacing.md,
                  AppSpacing.md,
                  AppSpacing.md,
                  0,
                ),
                child: _WeeklyProgressCard(
                  completed: weeklyCompleted,
                  total: weeklyTotal,
                  weekDaysDone: weekDaysDone,
                ),
              ),
            ),

            // â”€â”€ Today's Exercises â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(
                  AppSpacing.md,
                  AppSpacing.lg,
                  AppSpacing.md,
                  AppSpacing.sm,
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      l10n.todaysExercises,
                      style: AppTextStyles.headline.copyWith(
                        color: AppColors.textHeading,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    GestureDetector(
                      onTap: () => context.go('/exercises'),
                      child: Text(
                        l10n.seeAll,
                        style: AppTextStyles.footnote.copyWith(
                          color: AppColors.primary,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),

            exercisesAsync.when(
              data: (result) => result.fold(
                (_) => SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.md,
                    ),
                    child: EmptyState(
                      icon: Icons.error_outline_rounded,
                      title: l10n.couldntLoadExercises,
                      subtitle: l10n.pullDownToRetry,
                    ),
                  ),
                ),
                (exercises) {
                  if (exercises.isEmpty) {
                    return SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.symmetric(
                          horizontal: AppSpacing.md,
                        ),
                        child: EmptyState(
                          icon: Icons.fitness_center_outlined,
                          title: l10n.noExercisesToday,
                          subtitle: l10n.noExercisesTodaySubtitle,
                        ),
                      ),
                    );
                  }
                  final incomplete = exercises
                      .where((e) => e.feedback == null)
                      .toList();

                  if (incomplete.isEmpty) {
                    return SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.symmetric(
                          horizontal: AppSpacing.md,
                        ),
                        child: EmptyState(
                          icon: Icons.check_circle_outline_rounded,
                          title: l10n.allDoneToday,
                          subtitle: l10n.allDoneTodaySubtitle,
                        ),
                      ),
                    );
                  }

                  final preview = incomplete.take(3).toList();
                  return SliverPadding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.md,
                    ),
                    sliver: SliverList.separated(
                      itemCount: preview.length,
                      separatorBuilder: (_, _) =>
                          const SizedBox(height: AppSpacing.sm),
                      itemBuilder: (context, i) => ExerciseCard(
                        exercise: preview[i],
                        onTap: () =>
                            context.push('/exercise', extra: preview[i]),
                      ),
                    ),
                  );
                },
              ),
              loading: () => SliverPadding(
                padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
                sliver: SliverList.separated(
                  itemCount: 3,
                  separatorBuilder: (_, _) =>
                      const SizedBox(height: AppSpacing.sm),
                  itemBuilder: (_, _) => const ShimmerCard(height: 76),
                ),
              ),
              error: (_, _) => SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                    horizontal: AppSpacing.md,
                  ),
                  child: EmptyState(
                    icon: Icons.wifi_off_rounded,
                    title: l10n.somethingWentWrong,
                    subtitle: l10n.pullDownToRetry,
                  ),
                ),
              ),
            ),

            // â”€â”€ Next Appointment â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(
                  AppSpacing.md,
                  AppSpacing.lg,
                  AppSpacing.md,
                  AppSpacing.sm,
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      l10n.nextAppointmentTitle,
                      style: AppTextStyles.headline.copyWith(
                        color: AppColors.textHeading,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    GestureDetector(
                      onTap: () => context.go('/appointments'),
                      child: Text(
                        l10n.seeAll,
                        style: AppTextStyles.footnote.copyWith(
                          color: AppColors.primary,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),

            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
                child: appointmentsAsync.when(
                  loading: () => const ShimmerCard(height: 76),
                  error: (_, _) => const SizedBox.shrink(),
                  data: (_) => _NextAppointmentCard(appointment: nextAppointment),
                ),
              ),
            ),

            const SliverToBoxAdapter(child: SizedBox(height: AppSpacing.xl)),
          ],
        ),
      ),
    );
  }
}

// â”€â”€ Private Widgets â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

class _HeroHeader extends StatelessWidget {
  const _HeroHeader({required this.greeting, required this.date});

  final String greeting;
  final String date;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.fromLTRB(
        AppSpacing.md,
        AppSpacing.md,
        AppSpacing.md,
        AppSpacing.sm,
      ),
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF004AAD), Color(0xFF0062D9)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFF004AAD).withValues(alpha: 0.28),
            blurRadius: 24,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            date,
            style: AppTextStyles.footnote.copyWith(
              color: Colors.white.withValues(alpha: 0.65),
              letterSpacing: 0.2,
            ),
          ),
          const SizedBox(height: AppSpacing.xs),
          Text(
            greeting,
            style: AppTextStyles.title1.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.w800,
              letterSpacing: -0.5,
            ),
          ),
        ],
      ),
    );
  }
}

class _StatCard extends StatelessWidget {
  const _StatCard({
    required this.icon,
    required this.iconColor,
    required this.iconBg,
    required this.value,
    required this.label,
  });

  final IconData icon;
  final Color iconColor;
  final Color iconBg;
  final String value;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: AppSpacing.md,
      ),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 34,
            height: 34,
            decoration: BoxDecoration(
              color: iconBg,
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(icon, size: 17, color: iconColor),
          ),
          const SizedBox(height: 10),
          Text(
            value,
            style: AppTextStyles.headline.copyWith(
              color: AppColors.textPrimary,
              fontWeight: FontWeight.w700,
              fontSize: 16,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            style: AppTextStyles.caption.copyWith(
              color: AppColors.textSecondary,
              fontSize: 12,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      ),
    );
  }
}

class _WeeklyProgressCard extends StatelessWidget {
  const _WeeklyProgressCard({
    required this.completed,
    required this.total,
    required this.weekDaysDone,
  });

  final int completed;
  final int total;
  final Set<int> weekDaysDone; // weekday numbers 1=Monâ€¦7=Sun

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final ratio = total > 0 ? (completed / total).clamp(0.0, 1.0) : 0.0;
    final now = DateTime.now();
    final weekday = now.weekday; // 1=Mon … 7=Sun

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                l10n.weeklyProgress,
                style: AppTextStyles.headline.copyWith(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.w600,
                ),
              ),
              Text(
                l10n.sessionsProgress(completed, total),
                style: AppTextStyles.footnote.copyWith(
                  color: AppColors.textSecondary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          // Progress bar
          ClipRRect(
            borderRadius: BorderRadius.circular(4),
            child: LinearProgressIndicator(
              value: ratio,
              minHeight: 6,
              backgroundColor: AppColors.background,
              valueColor: const AlwaysStoppedAnimation(AppColors.secondary),
            ),
          ),
          const SizedBox(height: 14),
          // Day circles
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: List.generate(7, (i) {
              const labels = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];
              final dayNum = i + 1; // 1=Mon
              final isToday = dayNum == weekday;
              final isDone = weekDaysDone.contains(dayNum);

              return _DayCircle(
                label: labels[i],
                isCompleted: isDone,
                isToday: isToday && !isDone,
              );
            }),
          ),
        ],
      ),
    );
  }
}

class _DayCircle extends StatelessWidget {
  const _DayCircle({
    required this.label,
    required this.isCompleted,
    required this.isToday,
  });

  final String label;
  final bool isCompleted;
  final bool isToday;

  @override
  Widget build(BuildContext context) {
    if (isCompleted) {
      return Column(
        children: [
          Container(
            width: 34,
            height: 34,
            decoration: const BoxDecoration(
              color: AppColors.secondary,
              shape: BoxShape.circle,
            ),
            child: const Icon(
              Icons.check_rounded,
              size: 16,
              color: Colors.white,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            label,
            style: AppTextStyles.caption.copyWith(
              color: AppColors.secondary,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      );
    }

    if (isToday) {
      return Column(
        children: [
          Container(
            width: 34,
            height: 34,
            decoration: BoxDecoration(
              color: AppColors.primary,
              shape: BoxShape.circle,
              border: Border.all(color: AppColors.primary, width: 2),
            ),
            child: Center(
              child: Text(
                label,
                style: AppTextStyles.footnote.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ),
          const SizedBox(height: 4),
          Text(
            label,
            style: AppTextStyles.caption.copyWith(
              color: AppColors.primary,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      );
    }

    return Column(
      children: [
        Container(
          width: 34,
          height: 34,
          decoration: BoxDecoration(
            color: AppColors.background,
            shape: BoxShape.circle,
            border: Border.all(
              color: AppColors.divider,
              width: 1,
            ),
          ),
          child: Center(
            child: Text(
              label,
              style: AppTextStyles.footnote.copyWith(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
        ),
        const SizedBox(height: 4),
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

class _NextAppointmentCard extends StatelessWidget {
  const _NextAppointmentCard({required this.appointment});

  final Appointment? appointment;

  static const _months = [
    'JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN',
    'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC',
  ];

  String _formatTime(DateTime dt) {
    final h = dt.hour % 12 == 0 ? 12 : dt.hour % 12;
    final m = dt.minute.toString().padLeft(2, '0');
    final p = dt.hour >= 12 ? 'pm' : 'am';
    return '$h:$m $p';
  }

  String _statusLabel(AppLocalizations l10n, AppointmentStatus s) => switch (s) {
        AppointmentStatus.confirmed => l10n.statusConfirmed,
        AppointmentStatus.pending => l10n.statusPending,
        AppointmentStatus.cancelled => l10n.statusCancelled,
        AppointmentStatus.completed => l10n.statusDone,
      };

  Color _statusColor(AppointmentStatus s) => switch (s) {
        AppointmentStatus.confirmed => AppColors.secondary,
        AppointmentStatus.pending => const Color(0xFFF59E0B),
        AppointmentStatus.cancelled => AppColors.destructive,
        AppointmentStatus.completed => AppColors.textSecondary,
      };

  Color _statusBg(AppointmentStatus s) => switch (s) {
        AppointmentStatus.confirmed => AppColors.secondaryLight,
        AppointmentStatus.pending => const Color(0xFFFFF8E6),
        AppointmentStatus.cancelled => const Color(0xFFFFEEED),
        AppointmentStatus.completed => AppColors.background,
      };

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    // Empty state
    if (appointment == null) {
      return Container(
        padding: const EdgeInsets.all(AppSpacing.md),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Row(
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: AppColors.background,
                borderRadius: BorderRadius.circular(12),
              ),
              child: const Icon(
                Icons.calendar_today_outlined,
                size: 18,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    l10n.noUpcomingAppointments,
                    style: AppTextStyles.subheadline.copyWith(
                      color: AppColors.textPrimary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    l10n.tapToRequestSession,
                    style: AppTextStyles.caption.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      );
    }

    final time = appointment!.appointmentTime!;
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          // Date block
          Container(
            width: 48,
            padding: const EdgeInsets.symmetric(vertical: 8),
            decoration: BoxDecoration(
              color: AppColors.primaryLight,
              borderRadius: BorderRadius.circular(12),
            ),
            child: Column(
              children: [
                Text(
                  '${time.day}',
                  style: AppTextStyles.title2.copyWith(
                    color: AppColors.primary,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                Text(
                  _months[time.month - 1],
                  style: AppTextStyles.caption.copyWith(
                    color: AppColors.primary,
                    fontWeight: FontWeight.w600,
                    letterSpacing: 0.5,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  appointment!.title,
                  style: AppTextStyles.subheadline.copyWith(
                    color: AppColors.textPrimary,
                    fontWeight: FontWeight.w600,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 3),
                Row(
                  children: [
                    const Icon(
                      Icons.access_time_outlined,
                      size: 11,
                      color: AppColors.textSecondary,
                    ),
                    const SizedBox(width: 3),
                    Text(
                      _formatTime(time),
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(width: AppSpacing.sm),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
            decoration: BoxDecoration(
              color: _statusBg(appointment!.status),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              _statusLabel(l10n, appointment!.status),
              style: AppTextStyles.caption.copyWith(
                color: _statusColor(appointment!.status),
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
