import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/core/widgets/empty_state.dart';
import 'package:practice/core/widgets/shimmer_card.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';
import 'package:practice/features/appointments/presentation/providers/appointments_providers.dart';

class AppointmentsPage extends ConsumerWidget {
  const AppointmentsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final appointmentsAsync = ref.watch(appointmentsProvider);

    return SafeArea(
      child: CustomScrollView(
        slivers: [
          // â”€â”€ Header (Request button shown only when appointments exist) â”€â”€â”€â”€â”€â”€â”€
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
                    child: Text(
                      'Appointments',
                      style: AppTextStyles.title1.copyWith(
                        color: AppColors.textHeading,
                        fontWeight: FontWeight.w800,
                        letterSpacing: -0.5,
                      ),
                    ),
                  ),
                  // Only show when there are appointments (not empty / loading)
                  appointmentsAsync.when(
                    data: (result) => result.fold(
                      (_) => const SizedBox.shrink(),
                      (list) => list.isEmpty
                          ? const SizedBox.shrink()
                          : _RequestButton(
                              onTap: () => context.push('/appointment'),
                            ),
                    ),
                    loading: () => const SizedBox.shrink(),
                    error: (_, _) => const SizedBox.shrink(),
                  ),
                ],
              ),
            ),
          ),

          // â”€â”€ Body â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          appointmentsAsync.when(
            loading: () => SliverPadding(
              padding: const EdgeInsets.fromLTRB(
                AppSpacing.md,
                AppSpacing.lg,
                AppSpacing.md,
                0,
              ),
              sliver: SliverList.separated(
                itemCount: 4,
                separatorBuilder: (_, _) =>
                    const SizedBox(height: AppSpacing.sm),
                itemBuilder: (_, _) => const ShimmerCard(height: 88),
              ),
            ),
            error: (_, _) => SliverFillRemaining(
              child: EmptyState(
                icon: Icons.wifi_off_rounded,
                title: 'Something went wrong',
                subtitle: 'An unexpected error occurred.',
                ctaLabel: 'Retry',
                onCta: () => ref.invalidate(appointmentsProvider),
              ),
            ),
            data: (result) => result.fold(
              (failure) =>
                  SliverFillRemaining(child: _buildError(failure, ref)),
              (appointments) => _buildData(context, ref, appointments),
            ),
          ),

          const SliverToBoxAdapter(child: SizedBox(height: AppSpacing.xl)),
        ],
      ),
    );
  }

  Widget _buildError(AppFailure failure, WidgetRef ref) {
    final message = switch (failure) {
      NetworkFailure() => 'No internet connection',
      ServerFailure() => 'Server error. Try again.',
      AuthFailure() => 'Session expired. Please log in again.',
    };
    return EmptyState(
      icon: Icons.error_outline_rounded,
      title: "Couldn't load appointments",
      subtitle: message,
      ctaLabel: 'Retry',
      onCta: () => ref.invalidate(appointmentsProvider),
    );
  }

  Widget _buildData(
    BuildContext context,
    WidgetRef ref,
    List<Appointment> appointments,
  ) {
    final now = DateTime.now();

    final upcoming = appointments
        .where(
          (a) =>
              a.appointmentTime != null &&
              a.appointmentTime!.isAfter(now) &&
              a.status != AppointmentStatus.cancelled,
        )
        .toList()
      ..sort((a, b) => a.appointmentTime!.compareTo(b.appointmentTime!));

    final past = appointments
        .where(
          (a) =>
              (a.appointmentTime != null &&
                  a.appointmentTime!.isBefore(now)) ||
              a.status == AppointmentStatus.completed ||
              a.status == AppointmentStatus.cancelled,
        )
        .toList()
      ..sort((a, b) => b.appointmentTime!.compareTo(a.appointmentTime!));

    if (appointments.isEmpty) {
      return SliverFillRemaining(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.calendar_today_outlined,
              size: 52,
              color: AppColors.primary,
            ),
            const SizedBox(height: 16),
            Text(
              'Nothing on the books',
              style: AppTextStyles.headline.copyWith(
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Ready for your next session?\nRequest a time and your clinic will confirm.',
              style: AppTextStyles.subheadline.copyWith(
                color: AppColors.textSecondary,
                height: 1.5,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            GestureDetector(
              onTap: () => context.push('/appointment'),
              child: Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: AppSpacing.xl,
                  vertical: AppSpacing.sm + 4,
                ),
                decoration: BoxDecoration(
                  color: AppColors.primary,
                  borderRadius: BorderRadius.circular(AppRadius.pill),
                ),
                child: Text(
                  '+ Request appointment',
                  style: AppTextStyles.callout.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ),
          ],
        ),
      );
    }

    return SliverMainAxisGroup(
      slivers: [
        // â”€â”€ Upcoming section â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        if (upcoming.isNotEmpty) ...[
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(
                AppSpacing.md,
                AppSpacing.lg,
                AppSpacing.md,
                AppSpacing.sm,
              ),
              child: Text(
                'Upcoming آ· ${upcoming.length}',
                style: AppTextStyles.footnote.copyWith(
                  color: AppColors.textSecondary,
                  fontWeight: FontWeight.w600,
                  letterSpacing: 0.3,
                ),
              ),
            ),
          ),
          SliverPadding(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
            sliver: SliverList.separated(
              itemCount: upcoming.length,
              separatorBuilder: (_, _) => const SizedBox(height: AppSpacing.sm),
              itemBuilder: (_, i) =>
                  _AppointmentRow(appointment: upcoming[i], muted: false),
            ),
          ),
        ],

        // â”€â”€ Past section â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        if (past.isNotEmpty) ...[
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(
                AppSpacing.md,
                AppSpacing.lg,
                AppSpacing.md,
                AppSpacing.sm,
              ),
              child: Text(
                'Past آ· ${past.length}',
                style: AppTextStyles.footnote.copyWith(
                  color: AppColors.textSecondary,
                  fontWeight: FontWeight.w600,
                  letterSpacing: 0.3,
                ),
              ),
            ),
          ),
          SliverPadding(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
            sliver: SliverList.separated(
              itemCount: past.length,
              separatorBuilder: (_, _) => const SizedBox(height: AppSpacing.sm),
              itemBuilder: (_, i) =>
                  _AppointmentRow(appointment: past[i], muted: true),
            ),
          ),
        ],
      ],
    );
  }
}

// â”€â”€ Request button widget â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

class _RequestButton extends StatelessWidget {
  const _RequestButton({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
        decoration: BoxDecoration(
          color: AppColors.primary,
          borderRadius: BorderRadius.circular(AppRadius.pill),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.add_rounded, color: Colors.white, size: 15),
            const SizedBox(width: 4),
            Text(
              'Request',
              style: AppTextStyles.footnote.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// â”€â”€ Appointment row (matches design: date block + content + badge) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

class _AppointmentRow extends StatelessWidget {
  const _AppointmentRow({required this.appointment, required this.muted});

  final Appointment appointment;
  final bool muted;

  static const _months = [
    'JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN',
    'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC',
  ];

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

  String _statusLabel(AppointmentStatus s) => switch (s) {
        AppointmentStatus.confirmed => 'Confirmed',
        AppointmentStatus.pending => 'Pending',
        AppointmentStatus.cancelled => 'Cancelled',
        AppointmentStatus.completed => 'Done',
      };

  String _formatTime(DateTime dt) {
    final h = dt.hour % 12 == 0 ? 12 : dt.hour % 12;
    final m = dt.minute.toString().padLeft(2, '0');
    final p = dt.hour >= 12 ? 'pm' : 'am';
    final endDt = dt.add(const Duration(minutes: 45));
    final eh = endDt.hour % 12 == 0 ? 12 : endDt.hour % 12;
    final em = endDt.minute.toString().padLeft(2, '0');
    final ep = endDt.hour >= 12 ? 'pm' : 'am';
    return '$h:$m $p â€“ $eh:$em $ep';
  }

  @override
  Widget build(BuildContext context) {
    final time = appointment.appointmentTime;
    final statusColor = _statusColor(appointment.status);
    final dateNumColor = muted ? AppColors.textSecondary : AppColors.primary;

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          // Date block
          if (time != null)
            Column(
              children: [
                Text(
                  '${time.day}',
                  style: AppTextStyles.title2.copyWith(
                    color: dateNumColor,
                    fontWeight: FontWeight.w800,
                    fontSize: 24,
                    height: 1,
                  ),
                ),
                Text(
                  _months[time.month - 1],
                  style: AppTextStyles.caption.copyWith(
                    color: dateNumColor,
                    fontWeight: FontWeight.w600,
                    letterSpacing: 0.5,
                    fontSize: 11,
                  ),
                ),
              ],
            )
          else
            const SizedBox(width: 32),

          const SizedBox(width: AppSpacing.md),

          // Content
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  appointment.title,
                  style: AppTextStyles.subheadline.copyWith(
                    color: muted
                        ? AppColors.textSecondary
                        : AppColors.textPrimary,
                    fontWeight: FontWeight.w600,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                if (time != null) ...[
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(
                        Icons.access_time_outlined,
                        size: 12,
                        color: AppColors.textSecondary,
                      ),
                      const SizedBox(width: 4),
                      Text(
                        _formatTime(time),
                        style: AppTextStyles.caption.copyWith(
                          color: AppColors.textSecondary,
                        ),
                      ),
                    ],
                  ),
                ],
              ],
            ),
          ),

          const SizedBox(width: AppSpacing.sm),

          // Status badge
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
            decoration: BoxDecoration(
              color: _statusBg(appointment.status),
              borderRadius: BorderRadius.circular(AppRadius.pill),
            ),
            child: Text(
              _statusLabel(appointment.status),
              style: AppTextStyles.caption.copyWith(
                color: statusColor,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
