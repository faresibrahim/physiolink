import 'package:flutter/material.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/core/widgets/status_badge.dart';
import 'package:practice/features/appointments/domain/entities/appointment.dart';

class AppointmentTile extends StatelessWidget {
  const AppointmentTile({super.key, required this.appointment});

  final Appointment appointment;

  static Color _statusColor(AppointmentStatus status) => switch (status) {
    AppointmentStatus.confirmed => AppColors.secondary,
    AppointmentStatus.pending => AppColors.warning,
    AppointmentStatus.cancelled => AppColors.destructive,
    AppointmentStatus.completed => AppColors.textSecondary,
  };

  static String _statusLabel(AppointmentStatus status) => switch (status) {
    AppointmentStatus.confirmed => 'Confirmed',
    AppointmentStatus.pending => 'Pending',
    AppointmentStatus.cancelled => 'Cancelled',
    AppointmentStatus.completed => 'Done',
  };

  @override
  Widget build(BuildContext context) {
    final time = appointment.appointmentTime;

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          // Date column
          if (time != null) ...[
            _DateColumn(dateTime: time),
            const SizedBox(width: AppSpacing.md),
            Container(width: 1, height: 48, color: AppColors.divider),
            const SizedBox(width: AppSpacing.md),
          ],

          // Details
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  appointment.title,
                  style: AppTextStyles.headline,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: AppSpacing.xs),
                if (time != null)
                  Text(
                    _formatTime(time),
                    style: AppTextStyles.footnote.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
              ],
            ),
          ),

          const SizedBox(width: AppSpacing.sm),

          // Status badge
          StatusBadge(
            label: _statusLabel(appointment.status),
            color: _statusColor(appointment.status),
          ),
        ],
      ),
    );
  }

  String _formatTime(DateTime dt) {
    final hour = dt.hour % 12 == 0 ? 12 : dt.hour % 12;
    final minute = dt.minute.toString().padLeft(2, '0');
    final period = dt.hour >= 12 ? 'PM' : 'AM';
    return '$hour:$minute $period';
  }
}

class _DateColumn extends StatelessWidget {
  const _DateColumn({required this.dateTime});

  final DateTime dateTime;

  static const List<String> _months = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 36,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            dateTime.day.toString(),
            style: AppTextStyles.title2.copyWith(color: AppColors.primary),
          ),
          Text(_months[dateTime.month - 1], style: AppTextStyles.caption),
        ],
      ),
    );
  }
}
