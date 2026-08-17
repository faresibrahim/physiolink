import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:table_calendar/table_calendar.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/core/widgets/shimmer_card.dart';
import 'package:practice/features/appointments/domain/entities/appointment_slot.dart';
import 'package:practice/features/appointments/presentation/providers/appointments_providers.dart';
import 'package:practice/l10n/app_localizations.dart';

// Outcome bubbled up from the confirmation sheet so the page can flash the right
// message after it closes.
enum _BookingResult { success, taken }

// Slot-based booking, redesigned around a month calendar (task 4): only the days
// that actually have open slots ("working days") are selectable; tapping one
// reveals that day's times underneath. Themed to the house design system:
// 8pt spacing, type on the multiples-of-2 scale, 24px icons, >=44px tap targets,
// near-black text, flat surfaces, and a single isolated primary CTA in the sheet.
class SlotBookingPage extends ConsumerStatefulWidget {
  const SlotBookingPage({super.key});

  @override
  ConsumerState<SlotBookingPage> createState() => _SlotBookingPageState();
}

class _SlotBookingPageState extends ConsumerState<SlotBookingPage> {
  Future<void> _openSheet(AppointmentSlot slot) async {
    final result = await showModalBottomSheet<_BookingResult>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => _RequestSlotSheet(slot: slot),
    );
    if (!mounted || result == null) return;

    final l10n = AppLocalizations.of(context)!;
    if (result == _BookingResult.success) {
      _snack(l10n.requestSentPending, AppColors.secondary, Icons.check_circle_rounded);
    } else {
      ref.invalidate(availableSlotsProvider);
      _snack(l10n.slotTaken, AppColors.warning, Icons.info_rounded);
    }
  }

  void _snack(String message, Color color, IconData icon) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(
          content: Row(
            children: [
              Icon(icon, color: Colors.white, size: 20),
              const SizedBox(width: AppSpacing.sm),
              Expanded(child: Text(message, style: AppTextStyles.callout.copyWith(color: Colors.white))),
            ],
          ),
          backgroundColor: color,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(AppRadius.button)),
        ),
      );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final slotsAsync = ref.watch(availableSlotsProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _Header(title: l10n.bookAppointment, subtitle: l10n.chooseSlotSubtitle),
            Expanded(
              child: slotsAsync.when(
                loading: () => _SkeletonList(),
                error: (_, _) => _StateView(
                  icon: Icons.wifi_off_rounded,
                  title: l10n.somethingWentWrong,
                  subtitle: l10n.unexpectedError,
                  cta: l10n.retry,
                  onCta: () => ref.invalidate(availableSlotsProvider),
                ),
                data: (result) => result.fold(
                  (failure) => _StateView(
                    icon: Icons.error_outline_rounded,
                    title: l10n.somethingWentWrong,
                    subtitle: switch (failure) {
                      NetworkFailure() => l10n.noInternet,
                      AuthFailure() => l10n.sessionExpired,
                      ServerFailure() => l10n.serverError,
                    },
                    cta: l10n.retry,
                    onCta: () => ref.invalidate(availableSlotsProvider),
                  ),
                  (slots) => slots.isEmpty
                      ? _StateView(
                          icon: Icons.event_available_rounded,
                          title: l10n.noSlotsTitle,
                          subtitle: l10n.noSlotsSubtitle,
                          cta: l10n.retry,
                          onCta: () => ref.invalidate(availableSlotsProvider),
                        )
                      : _CalendarBookingView(slots: slots, onTap: _openSheet),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ── Header (large iOS-style title) ──────────────────────────────────────────
class _Header extends StatelessWidget {
  const _Header({required this.title, required this.subtitle});

  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(AppSpacing.lg, AppSpacing.sm, AppSpacing.lg, AppSpacing.md),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 44x44 flat back button (Fitts: reachable, finger-sized).
          Material(
            color: AppColors.surface,
            shape: const CircleBorder(),
            child: InkWell(
              customBorder: const CircleBorder(),
              onTap: () => Navigator.of(context).pop(),
              child: const SizedBox(
                width: 44,
                height: 44,
                child: Icon(Icons.arrow_back_ios_new_rounded, size: 20, color: AppColors.textPrimary),
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          Text(title, style: AppTextStyles.largeTitle),
          const SizedBox(height: AppSpacing.xs),
          Text(subtitle, style: AppTextStyles.subheadline.copyWith(color: AppColors.textSecondary)),
        ],
      ),
    );
  }
}

// ── Calendar + selected-day slots ───────────────────────────────────────────
class _CalendarBookingView extends StatefulWidget {
  const _CalendarBookingView({required this.slots, required this.onTap});

  final List<AppointmentSlot> slots;
  final void Function(AppointmentSlot) onTap;

  @override
  State<_CalendarBookingView> createState() => _CalendarBookingViewState();
}

class _CalendarBookingViewState extends State<_CalendarBookingView> {
  DateTime? _selectedDay;
  DateTime? _focusedDay;

  // Slot times are UTC; the app treats the raw wall clock of scheduledAt as the
  // clinic's operating-hours time, so day matching uses the raw y/m/d too.
  DateTime _dateOnly(DateTime dt) => DateTime.utc(dt.year, dt.month, dt.day);

  bool _hasSlots(DateTime day) =>
      widget.slots.any((s) => isSameDay(s.scheduledAt, day));

  List<AppointmentSlot> _slotsForDay(DateTime day) =>
      widget.slots.where((s) => isSameDay(s.scheduledAt, day)).toList()
        ..sort((a, b) => a.scheduledAt.compareTo(b.scheduledAt));

  // Prefer landing on the first day that has a bookable slot; if every remaining
  // day is fully booked, fall back to the first day so booked times still show.
  DateTime _defaultDay(List<DateTime> available) {
    for (final d in available) {
      if (widget.slots.any((s) => s.isAvailable && isSameDay(s.scheduledAt, d))) {
        return d;
      }
    }
    return available.first;
  }

  // Distinct days that have at least one open slot, ascending.
  List<DateTime> get _availableDays {
    final byKey = <String, DateTime>{};
    for (final s in widget.slots) {
      final d = _dateOnly(s.scheduledAt);
      byKey['${d.year}-${d.month}-${d.day}'] = d;
    }
    return byKey.values.toList()..sort();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final locale = Localizations.localeOf(context).toString();

    final available = _availableDays; // non-empty: parent only builds us with slots
    final today = _dateOnly(DateTime.now());

    // Keep the user's pick unless it no longer has slots (e.g. after booking),
    // then fall back to the first available day.
    final selected = (_selectedDay != null && _hasSlots(_selectedDay!))
        ? _selectedDay!
        : _defaultDay(available);

    // Calendar bounds must span both today and every available day.
    final firstDay = available.first.isBefore(today) ? available.first : today;
    final horizon = today.add(const Duration(days: 90));
    final lastDay = available.last.isAfter(horizon) ? available.last : horizon;

    // Keep the focused month inside the bounds (guards against a stale focus
    // after the slot list refreshes and the bounds shift).
    var focused = _focusedDay ?? selected;
    if (focused.isBefore(firstDay)) focused = firstDay;
    if (focused.isAfter(lastDay)) focused = lastDay;

    final daySlots = _slotsForDay(selected);

    // 3-column grid: subtract the two 8px gaps from the content width, then divide.
    final contentWidth = MediaQuery.of(context).size.width - AppSpacing.lg * 2;
    final cardWidth = (contentWidth - AppSpacing.sm * 2) / 3;

    return ListView(
      padding: const EdgeInsets.only(bottom: AppSpacing.xl),
      children: [
        // Calendar card
        Container(
          margin: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
          padding: const EdgeInsets.symmetric(horizontal: AppSpacing.sm, vertical: AppSpacing.xs),
          decoration: BoxDecoration(
            color: AppColors.surface,
            borderRadius: BorderRadius.circular(20),
            border: Border.all(color: AppColors.divider),
          ),
          child: TableCalendar<AppointmentSlot>(
            firstDay: firstDay,
            lastDay: lastDay,
            focusedDay: focused,
            currentDay: today,
            locale: locale,
            startingDayOfWeek: StartingDayOfWeek.monday,
            calendarFormat: CalendarFormat.month,
            availableGestures: AvailableGestures.horizontalSwipe,
            rowHeight: 56,
            daysOfWeekHeight: 28,
            sixWeekMonthsEnforced: false,
            selectedDayPredicate: (d) => isSameDay(d, selected),
            enabledDayPredicate: _hasSlots,
            // Dot under a day = it still has open times. Days that exist but are
            // fully booked stay selectable, just without the dot.
            eventLoader: (day) => widget.slots
                .where((s) => s.isAvailable && isSameDay(s.scheduledAt, day))
                .toList(),
            calendarBuilders: CalendarBuilders<AppointmentSlot>(
              markerBuilder: (context, day, events) {
                if (events.isEmpty) return null;
                return Container(
                  width: 5,
                  height: 5,
                  margin: const EdgeInsets.only(bottom: AppSpacing.xs),
                  decoration: const BoxDecoration(
                    color: AppColors.secondary,
                    shape: BoxShape.circle,
                  ),
                );
              },
            ),
            onDaySelected: (sel, foc) {
              setState(() {
                _selectedDay = _dateOnly(sel);
                _focusedDay = foc;
              });
            },
            onPageChanged: (foc) => _focusedDay = foc,
            headerStyle: HeaderStyle(
              formatButtonVisible: false,
              titleCentered: true,
              headerPadding: const EdgeInsets.symmetric(vertical: AppSpacing.sm),
              titleTextStyle: AppTextStyles.headline,
              leftChevronIcon: const Icon(Icons.chevron_left_rounded, color: AppColors.primary),
              rightChevronIcon: const Icon(Icons.chevron_right_rounded, color: AppColors.primary),
            ),
            daysOfWeekStyle: DaysOfWeekStyle(
              weekdayStyle: AppTextStyles.caption.copyWith(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
              weekendStyle: AppTextStyles.caption.copyWith(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
            ),
            calendarStyle: CalendarStyle(
              outsideDaysVisible: false,
              // Caps the day circle at 32px so the availability dot below it
              // always clears, on phone and tablet widths alike.
              cellMargin: const EdgeInsets.symmetric(horizontal: 6, vertical: 12),
              defaultTextStyle: AppTextStyles.callout.copyWith(color: AppColors.textPrimary),
              weekendTextStyle: AppTextStyles.callout.copyWith(color: AppColors.textPrimary),
              disabledTextStyle: AppTextStyles.callout.copyWith(
                color: AppColors.textSecondary.withValues(alpha: 0.35),
              ),
              todayDecoration: const BoxDecoration(
                color: AppColors.primaryLight,
                shape: BoxShape.circle,
              ),
              todayTextStyle: AppTextStyles.callout.copyWith(
                color: AppColors.primary,
                fontWeight: FontWeight.w700,
              ),
              selectedDecoration: const BoxDecoration(
                color: AppColors.primary,
                shape: BoxShape.circle,
              ),
              selectedTextStyle: AppTextStyles.callout.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
          ),
        ),

        const SizedBox(height: AppSpacing.lg),

        // Selected-day heading
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Text(l10n.chooseTimeTitle, style: AppTextStyles.title2),
                  const SizedBox(width: AppSpacing.sm),
                  // Session length stated once here instead of on every card.
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.sm,
                      vertical: 2,
                    ),
                    decoration: BoxDecoration(
                      color: AppColors.primaryLight,
                      borderRadius: BorderRadius.circular(AppRadius.pill),
                    ),
                    child: Text(
                      l10n.slotDurationHint,
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.primary,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.xs),
              Text(
                '${DateFormat.EEEE(locale).format(selected)}, ${DateFormat.MMMMd(locale).format(selected)}',
                style: AppTextStyles.footnote.copyWith(color: AppColors.textSecondary),
              ),
            ],
          ),
        ),

        const SizedBox(height: AppSpacing.md),

        // Selected-day slots — cross-faded so switching days reads as a change
        // of content rather than an instant swap.
        AnimatedSwitcher(
          duration: const Duration(milliseconds: 260),
          switchInCurve: Curves.easeOutCubic,
          transitionBuilder: (child, anim) => FadeTransition(
            opacity: anim,
            child: SlideTransition(
              position: Tween(
                begin: const Offset(0, 0.06),
                end: Offset.zero,
              ).animate(anim),
              child: child,
            ),
          ),
          child: Padding(
            key: ValueKey(selected),
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
            child: Wrap(
              spacing: AppSpacing.sm,
              runSpacing: AppSpacing.sm,
              children: [
                for (final slot in daySlots)
                  SizedBox(
                    width: cardWidth,
                    child: _SlotCard(
                      time: DateFormat.jm(locale).format(slot.scheduledAt),
                      bookedLabel: l10n.slotBooked,
                      available: slot.isAvailable,
                      onTap: slot.isAvailable ? () => widget.onTap(slot) : null,
                    ),
                  ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

// ── One time card (flat, >=44px tall) ───────────────────────────────────────
// Open times carry the brand tint so the grid reads as a set of pickable
// choices; booked times recede to a muted, struck-through slab — kept on screen
// so a taken time reads as "Booked" rather than vanishing.
class _SlotCard extends StatefulWidget {
  const _SlotCard({
    required this.time,
    required this.bookedLabel,
    required this.available,
    required this.onTap,
  });

  final String time;
  final String bookedLabel;
  final bool available;
  final VoidCallback? onTap;

  @override
  State<_SlotCard> createState() => _SlotCardState();
}

class _SlotCardState extends State<_SlotCard> {
  bool _pressed = false;

  void _setPressed(bool value) {
    if (_pressed != value) setState(() => _pressed = value);
  }

  @override
  Widget build(BuildContext context) {
    final available = widget.available;

    // A shade deeper than the brand green, used only as the far end of the
    // pressed-state gradient so the fill has real depth instead of one flat hue.
    final secondaryDeep = Color.lerp(AppColors.secondary, Colors.black, 0.16)!;

    final card = AnimatedContainer(
      duration: const Duration(milliseconds: 150),
      curve: Curves.easeOut,
      height: 56,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        gradient: available
            ? LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: _pressed
                    ? [AppColors.secondary, secondaryDeep]
                    : [Colors.white, AppColors.secondaryLight],
              )
            : null,
        color: available ? null : AppColors.background,
        borderRadius: BorderRadius.circular(AppRadius.card),
        border: Border.all(
          color: available
              ? AppColors.secondary.withValues(alpha: _pressed ? 0 : 0.3)
              : AppColors.divider,
        ),
        boxShadow: available && !_pressed
            ? [
                BoxShadow(
                  color: AppColors.secondary.withValues(alpha: 0.18),
                  blurRadius: 14,
                  offset: const Offset(0, 5),
                ),
              ]
            : null,
      ),
      child: available
          ? Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                AnimatedSwitcher(
                  duration: const Duration(milliseconds: 150),
                  child: Icon(
                    Icons.watch_later_rounded,
                    key: ValueKey(_pressed),
                    size: 15,
                    color: _pressed ? Colors.white : AppColors.secondary,
                  ),
                ),
                const SizedBox(width: 6),
                AnimatedDefaultTextStyle(
                  duration: const Duration(milliseconds: 150),
                  curve: Curves.easeOut,
                  style: AppTextStyles.headline.copyWith(
                    color: _pressed ? Colors.white : AppColors.secondary,
                    fontWeight: FontWeight.w700,
                  ),
                  child: Text(widget.time),
                ),
              ],
            )
          : Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  widget.time,
                  style: AppTextStyles.subheadline.copyWith(
                    color: AppColors.textSecondary,
                    decoration: TextDecoration.lineThrough,
                    decorationColor: AppColors.textSecondary,
                  ),
                ),
                const SizedBox(height: 1),
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.lock_rounded,
                      size: 10,
                      color: AppColors.textSecondary,
                    ),
                    const SizedBox(width: 3),
                    Text(
                      widget.bookedLabel,
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ],
            ),
    );

    // Booked slots render with no gesture handling at all, so they are visibly
    // present but completely unclickable.
    if (!available) {
      return Semantics(
        enabled: false,
        label: '${widget.time}, ${widget.bookedLabel}',
        child: ExcludeSemantics(child: card),
      );
    }

    return Semantics(
      button: true,
      label: widget.time,
      child: GestureDetector(
        onTap: widget.onTap,
        onTapDown: (_) => _setPressed(true),
        onTapUp: (_) => _setPressed(false),
        onTapCancel: () => _setPressed(false),
        child: AnimatedScale(
          scale: _pressed ? 0.96 : 1,
          duration: const Duration(milliseconds: 120),
          curve: Curves.easeOut,
          child: ExcludeSemantics(child: card),
        ),
      ),
    );
  }
}

// ── Loading skeleton ────────────────────────────────────────────────────────
class _SkeletonList extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final contentWidth = MediaQuery.of(context).size.width - AppSpacing.lg * 2;
    final cardWidth = (contentWidth - AppSpacing.sm * 2) / 3;

    return ListView(
      padding: const EdgeInsets.fromLTRB(AppSpacing.lg, AppSpacing.sm, AppSpacing.lg, AppSpacing.xl),
      children: [
        // Calendar placeholder
        ShimmerCard(height: 320, width: contentWidth),
        const SizedBox(height: AppSpacing.lg),
        const ShimmerCard(height: 16, width: 160),
        const SizedBox(height: AppSpacing.md),
        Wrap(
          spacing: AppSpacing.sm,
          runSpacing: AppSpacing.sm,
          children: [for (var i = 0; i < 6; i++) ShimmerCard(height: 56, width: cardWidth)],
        ),
      ],
    );
  }
}

// ── Empty / error state ─────────────────────────────────────────────────────
class _StateView extends StatelessWidget {
  const _StateView({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.cta,
    required this.onCta,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final String cta;
  final VoidCallback onCta;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 96,
              height: 96,
              decoration: const BoxDecoration(color: AppColors.primaryLight, shape: BoxShape.circle),
              child: Icon(icon, size: 40, color: AppColors.primary),
            ),
            const SizedBox(height: AppSpacing.lg),
            Text(title, style: AppTextStyles.title2, textAlign: TextAlign.center),
            const SizedBox(height: AppSpacing.sm),
            Text(
              subtitle,
              style: AppTextStyles.subheadline.copyWith(color: AppColors.textSecondary, height: 1.5),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: AppSpacing.lg),
            TextButton(
              onPressed: onCta,
              style: TextButton.styleFrom(
                minimumSize: const Size(88, 44),
                foregroundColor: AppColors.primary,
              ),
              child: Text(cta, style: AppTextStyles.headline.copyWith(color: AppColors.primary)),
            ),
          ],
        ),
      ),
    );
  }
}

// ── Confirmation bottom sheet — the single isolated primary CTA ─────────────
class _RequestSlotSheet extends ConsumerStatefulWidget {
  const _RequestSlotSheet({required this.slot});

  final AppointmentSlot slot;

  @override
  ConsumerState<_RequestSlotSheet> createState() => _RequestSlotSheetState();
}

class _RequestSlotSheetState extends ConsumerState<_RequestSlotSheet> {
  final _notesController = TextEditingController();
  bool _submitting = false;
  String? _error;

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _submitting = true;
      _error = null;
    });

    final notes = _notesController.text.trim();
    final failure = await ref.read(slotRequestProvider.notifier).request(
          slotId: widget.slot.slotId,
          notes: notes.isEmpty ? null : notes,
        );

    if (!mounted) return;

    if (failure == null) {
      Navigator.of(context).pop(_BookingResult.success);
      return;
    }
    if (failure is SlotUnavailableFailure) {
      Navigator.of(context).pop(_BookingResult.taken);
      return;
    }

    final l10n = AppLocalizations.of(context)!;
    setState(() {
      _submitting = false;
      _error = switch (failure) {
        NetworkFailure() => l10n.noInternetPeriod,
        AuthFailure() => l10n.sessionExpired,
        ServerFailure(:final statusCode) => l10n.serverErrorWithCode('${statusCode ?? ''}'),
      };
    });
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final locale = Localizations.localeOf(context).toString();
    final slot = widget.slot;
    final bottomInset = MediaQuery.of(context).viewInsets.bottom;

    return Padding(
      padding: EdgeInsets.only(bottom: bottomInset),
      child: Container(
        decoration: const BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.vertical(top: Radius.circular(AppSpacing.lg)),
        ),
        padding: const EdgeInsets.fromLTRB(AppSpacing.lg, AppSpacing.md, AppSpacing.lg, AppSpacing.lg),
        child: SafeArea(
          top: false,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Grab handle
              Center(
                child: Container(
                  width: 40,
                  height: 4,
                  decoration: BoxDecoration(
                    color: AppColors.divider,
                    borderRadius: BorderRadius.circular(AppRadius.pill),
                  ),
                ),
              ),
              const SizedBox(height: AppSpacing.lg),

              // Selected time, front and centre
              Row(
                children: [
                  Container(
                    width: 56,
                    height: 56,
                    decoration: const BoxDecoration(color: AppColors.primaryLight, shape: BoxShape.circle),
                    child: const Icon(Icons.event_rounded, size: 24, color: AppColors.primary),
                  ),
                  const SizedBox(width: AppSpacing.md),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${DateFormat.EEEE(locale).format(slot.scheduledAt)}, ${DateFormat.MMMMd(locale).format(slot.scheduledAt)}',
                          style: AppTextStyles.title2,
                        ),
                        const SizedBox(height: 2),
                        Text(
                          '${DateFormat.jm(locale).format(slot.scheduledAt)} · ${l10n.slotDurationHint}',
                          style: AppTextStyles.subheadline.copyWith(color: AppColors.primary, fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.lg),

              // Optional note
              Text(
                l10n.notesOptional,
                style: AppTextStyles.footnote.copyWith(color: AppColors.textSecondary, fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: AppSpacing.sm),
              TextField(
                controller: _notesController,
                maxLines: 3,
                enabled: !_submitting,
                style: AppTextStyles.body,
                decoration: InputDecoration(
                  hintText: l10n.notesHint,
                  hintStyle: AppTextStyles.body.copyWith(color: AppColors.textSecondary),
                  filled: true,
                  fillColor: AppColors.background,
                  contentPadding: const EdgeInsets.all(AppSpacing.md),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(AppRadius.button),
                    borderSide: BorderSide.none,
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(AppRadius.button),
                    borderSide: const BorderSide(color: AppColors.primary, width: 1.5),
                  ),
                ),
              ),
              const SizedBox(height: AppSpacing.md),

              // Honest expectation (no notifications yet — status honesty).
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Icon(Icons.info_outline_rounded, size: 16, color: AppColors.textSecondary),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: Text(
                      l10n.therapistWillConfirm,
                      style: AppTextStyles.caption,
                    ),
                  ),
                ],
              ),

              if (_error != null) ...[
                const SizedBox(height: AppSpacing.md),
                Row(
                  children: [
                    const Icon(Icons.error_outline_rounded, size: 16, color: AppColors.destructive),
                    const SizedBox(width: AppSpacing.sm),
                    Expanded(
                      child: Text(_error!, style: AppTextStyles.footnote.copyWith(color: AppColors.destructive)),
                    ),
                  ],
                ),
              ],
              const SizedBox(height: AppSpacing.lg),

              // Primary CTA — full width, 52px, isolated (Von Restorff).
              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed: _submitting ? null : _submit,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: Colors.white,
                    disabledBackgroundColor: AppColors.primary.withValues(alpha: 0.5),
                    elevation: 0,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(AppRadius.button)),
                  ),
                  child: _submitting
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                        )
                      : Text(
                          l10n.requestSlotCta,
                          style: AppTextStyles.headline.copyWith(color: Colors.white),
                        ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
