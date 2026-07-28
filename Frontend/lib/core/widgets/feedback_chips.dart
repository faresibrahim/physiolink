import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/l10n/app_localizations.dart';

class FeedbackChips extends StatefulWidget {
  const FeedbackChips({
    super.key,
    required this.selected,
    required this.onChanged,
  });

  final int? selected;
  final ValueChanged<int> onChanged;

  // Effort levels 1–10. Display labels are resolved via [labelFor] so they
  // localize; this list only drives the count/iteration of the chip rail.
  static const List<int> levels = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

  static String labelFor(AppLocalizations l10n, int value) => switch (value) {
        1 => l10n.effort1,
        2 => l10n.effort2,
        3 => l10n.effort3,
        4 => l10n.effort4,
        5 => l10n.effort5,
        6 => l10n.effort6,
        7 => l10n.effort7,
        8 => l10n.effort8,
        9 => l10n.effort9,
        10 => l10n.effort10,
        _ => l10n.effortLevel(value),
      };

  static Color colorFor(int value) {
    if (value <= 3) return const Color(0xFF4CAF82);
    if (value <= 5) return const Color(0xFFF59E0B);
    if (value <= 7) return const Color(0xFFFF6B35);
    return AppColors.destructive;
  }

  @override
  State<FeedbackChips> createState() => _FeedbackChipsState();
}

class _FeedbackChipsState extends State<FeedbackChips> {
  late final ScrollController _scrollController;

  @override
  void initState() {
    super.initState();
    _scrollController = ScrollController();
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final selected = widget.selected;
    final activeColor = selected != null
        ? FeedbackChips.colorFor(selected)
        : AppColors.textSecondary;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // ── Label row ────────────────────────────────────────
        AnimatedSwitcher(
          duration: const Duration(milliseconds: 200),
          transitionBuilder: (child, animation) => FadeTransition(
            opacity: animation,
            child: SlideTransition(
              position: Tween<Offset>(
                begin: const Offset(0, 0.2),
                end: Offset.zero,
              ).animate(animation),
              child: child,
            ),
          ),
          child: Row(
            key: ValueKey(selected),
            children: [
              if (selected != null) ...[
                Container(
                  width: 9,
                  height: 9,
                  decoration: BoxDecoration(
                    color: activeColor,
                    shape: BoxShape.circle,
                  ),
                ),
                const SizedBox(width: AppSpacing.xs),
                Text(
                  l10n.selectedEffort(
                    selected,
                    FeedbackChips.labelFor(l10n, selected),
                  ),
                  style: AppTextStyles.subheadline.copyWith(
                    color: activeColor,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ] else
                Text(
                  l10n.howIntenseWasIt,
                  style: AppTextStyles.subheadline.copyWith(
                    color: AppColors.textSecondary,
                    fontWeight: FontWeight.w400,
                  ),
                ),
            ],
          ),
        ),

        const SizedBox(height: AppSpacing.md),

        // ── Horizontal chip rail ─────────────────────────────
        SizedBox(
          height: 60,
          child: ListView.separated(
            controller: _scrollController,
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsetsDirectional.only(
              start: 2,
              end: AppSpacing.sm,
              top: 4,
              bottom: 4,
            ),
            itemCount: FeedbackChips.levels.length,
            separatorBuilder: (_, _) => const SizedBox(width: AppSpacing.sm),
            itemBuilder: (context, index) {
              final value = FeedbackChips.levels[index];
              final isSelected = selected == value;
              final chipColor = FeedbackChips.colorFor(value);

              return Semantics(
                label: FeedbackChips.labelFor(l10n, value),
                button: true,
                selected: isSelected,
                child: GestureDetector(
                  onTap: () {
                    HapticFeedback.selectionClick();
                    widget.onChanged(value);
                  },
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 200),
                    curve: Curves.easeInOut,
                    width: 52,
                    height: 52,
                    decoration: BoxDecoration(
                      color: isSelected
                          ? chipColor
                          : chipColor.withValues(alpha: 0.10),
                      shape: BoxShape.circle,
                      border: Border.all(
                        color: isSelected
                            ? chipColor
                            : chipColor.withValues(alpha: 0.28),
                        width: 1.5,
                      ),
                      boxShadow: isSelected
                          ? [
                              BoxShadow(
                                color: chipColor.withValues(alpha: 0.32),
                                blurRadius: 10,
                                offset: const Offset(0, 3),
                              ),
                            ]
                          : null,
                    ),
                    child: Center(
                      child: Text(
                        '$value',
                        style: AppTextStyles.title2.copyWith(
                          color: isSelected ? Colors.white : chipColor,
                          fontWeight: isSelected
                              ? FontWeight.w700
                              : FontWeight.w500,
                        ),
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
        ),

        const SizedBox(height: AppSpacing.xs),

        // ── Zone progress bar ────────────────────────────────
        _ZoneBar(selected: selected),

        const SizedBox(height: AppSpacing.xs),

        // ── Legend ───────────────────────────────────────────
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            _LegendLabel(label: l10n.legendEasy, color: const Color(0xFF4CAF82)),
            _LegendLabel(
              label: l10n.legendModerate,
              color: const Color(0xFFF59E0B),
            ),
            _LegendLabel(label: l10n.legendMaximum, color: AppColors.destructive),
          ],
        ),
      ],
    );
  }
}

// ── Zone progress bar ────────────────────────────────────────────────

class _ZoneBar extends StatelessWidget {
  const _ZoneBar({required this.selected});

  final int? selected;

  static const List<(int, Color)> _zones = [
    (3, Color(0xFF4CAF82)),
    (2, Color(0xFFF59E0B)),
    (2, Color(0xFFFF6B35)),
    (3, AppColors.destructive),
  ];

  @override
  Widget build(BuildContext context) {
    final int filled = selected ?? 0;

    // Flatten zones into individual segments: (color, index)
    final segments = <(Color, int)>[];
    for (final (count, color) in _zones) {
      for (int i = 0; i < count; i++) {
        segments.add((color, segments.length));
      }
    }

    return Row(
      children: [
        for (int i = 0; i < segments.length; i++) ...[
          if (i > 0) const SizedBox(width: 2),
          Expanded(
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              curve: Curves.easeOut,
              height: 3,
              decoration: BoxDecoration(
                color: segments[i].$1.withValues(
                  alpha: i < filled ? 1.0 : 0.22,
                ),
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
        ],
      ],
    );
  }
}

// ── Legend label ─────────────────────────────────────────────────────

class _LegendLabel extends StatelessWidget {
  const _LegendLabel({required this.label, required this.color});

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Text(
      label,
      style: AppTextStyles.caption.copyWith(
        color: color,
        fontWeight: FontWeight.w600,
        letterSpacing: 0.2,
      ),
    );
  }
}
