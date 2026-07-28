import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/features/appointments/presentation/providers/appointments_providers.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';
import 'package:practice/l10n/app_localizations.dart';

// Changed from StatefulWidget → ConsumerStatefulWidget so we can read
// Riverpod providers (the repository and the logged-in patient ID) inside state.
class AppointmentRequestPage extends ConsumerStatefulWidget {
  const AppointmentRequestPage({super.key});

  @override
  ConsumerState<AppointmentRequestPage> createState() =>
      _AppointmentRequestPageState();
}

class _AppointmentRequestPageState
    extends ConsumerState<AppointmentRequestPage> {
  final _notesController = TextEditingController();
  final _dateController = TextEditingController();
  final _timeController = TextEditingController();
  final _notesFocusNode = FocusNode();
  final _formKey = GlobalKey<FormState>();
  DateTime? _selectedDate;
  TimeOfDay? _selectedTime;
  bool _isLoading = false;
  String? _errorMessage;

  @override
  void dispose() {
    _notesController.dispose();
    _dateController.dispose();
    _timeController.dispose();
    _notesFocusNode.dispose();
    super.dispose();
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now(),
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 90)),
      builder: (context, child) => Theme(
        data: Theme.of(context).copyWith(
          colorScheme: const ColorScheme.light(
            primary: AppColors.primary,
            onPrimary: Colors.white,
            surface: AppColors.surface,
          ),
        ),
        child: child!,
      ),
    );
    if (picked != null) {
      setState(() {
        _selectedDate = picked;
        _dateController.text = '${picked.day}/${picked.month}/${picked.year}';
      });
    }
  }

  Future<void> _pickTime() async {
    final picked = await showTimePicker(
      context: context,
      initialTime: TimeOfDay.now(),
      builder: (context, child) => Theme(
        data: Theme.of(context).copyWith(
          colorScheme: const ColorScheme.light(
            primary: AppColors.primary,
            onPrimary: Colors.white,
          ),
        ),
        child: child!,
      ),
    );
    if (picked != null && mounted) {
      setState(() {
        _selectedTime = picked;
        _timeController.text = picked.format(context);
      });
    }
  }

  Future<void> _submit() async {
    // Clear any previous error before a new attempt.
    setState(() => _errorMessage = null);

    // Guard: date and time must be picked before form validation runs.
    if (_selectedDate == null || _selectedTime == null) {
      if (!_formKey.currentState!.validate()) return;
      return;
    }
    if (!_formKey.currentState!.validate()) return;

    // Combine the picked date and time into one DateTime that the API expects.
    // TimeOfDay only holds hour/minute, so we copy them onto the chosen date.
    final scheduledAt = DateTime(
      _selectedDate!.year,
      _selectedDate!.month,
      _selectedDate!.day,
      _selectedTime!.hour,
      _selectedTime!.minute,
    );

    // Read the logged-in patient's ID from the auth notifier.
    // We use ref.read (not ref.watch) because this is inside an async function,
    // not inside build() — we only need the value once at call time.
    final l10n = AppLocalizations.of(context)!;
    final patientId = ref.read(authNotifierProvider).patientId;
    if (patientId == null) {
      setState(() => _errorMessage = l10n.sessionExpired);
      return;
    }

    setState(() => _isLoading = true);
    try {
      final repository = ref.read(appointmentsRepositoryProvider);

      // requestAppointment returns Either<AppFailure, void>.
      // fold() lets us handle both the Left (failure) and Right (success) cases.
      final result = await repository.requestAppointment(
        patientId: patientId,
        scheduledAt: scheduledAt,
        notes: _notesController.text.trim().isEmpty
            ? null
            : _notesController.text.trim(),
      );

      if (!mounted) return;

      result.fold(
        (failure) {
          // Left branch — something went wrong. Map the failure type to a
          // human-readable message and display it inline (same pattern as login).
          final message = switch (failure) {
            NetworkFailure() => l10n.noInternetPeriod,
            AuthFailure() => l10n.sessionExpired,
            ServerFailure(:final statusCode) =>
              l10n.serverErrorWithCode('${statusCode ?? ''}'),
          };
          setState(() => _errorMessage = message);
        },
        (_) {
          // Right branch — success. Invalidate the appointments list so the
          // FutureProvider re-fetches and the new request appears immediately
          // when the user navigates back.
          ref.invalidate(appointmentsProvider);
          Navigator.of(context).pop();
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(l10n.appointmentRequested),
              backgroundColor: AppColors.secondary,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          );
        },
      );
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.surface,
        elevation: 0,
        scrolledUnderElevation: 0,
        leading: GestureDetector(
          behavior: HitTestBehavior.opaque,
          onTap: () => Navigator.of(context).pop(),
          child: const SizedBox(
            width: 44,
            height: 44,
            child: Center(
              child: Icon(
                Icons.arrow_back_ios_rounded,
                color: AppColors.textPrimary,
                size: 20,
              ),
            ),
          ),
        ),
        title: Text(l10n.requestAppointment, style: AppTextStyles.headline),
        bottom: const PreferredSize(
          preferredSize: Size.fromHeight(1),
          child: Divider(height: 1),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: AppSpacing.sm),

              // ── Date field ─────────────────────────────────────────────
              _FieldLabel(label: l10n.dateLabel),
              const SizedBox(height: AppSpacing.xs),
              TextFormField(
                controller: _dateController,
                onTap: _pickDate,
                readOnly: true,
                validator: (value) => (value == null || value.isEmpty)
                    ? l10n.pleaseSelectDate
                    : null,
                decoration: _fieldDecoration(
                  hint: l10n.selectDate,
                  icon: Icons.calendar_today_outlined,
                ),
              ),

              const SizedBox(height: AppSpacing.md),

              // ── Time field ─────────────────────────────────────────────
              _FieldLabel(label: l10n.timeLabel),
              const SizedBox(height: AppSpacing.xs),
              TextFormField(
                controller: _timeController,
                onTap: _pickTime,
                readOnly: true,
                validator: (value) => (value == null || value.isEmpty)
                    ? l10n.pleaseSelectTime
                    : null,
                decoration: _fieldDecoration(
                  hint: l10n.selectTime,
                  icon: Icons.access_time_rounded,
                ),
              ),

              const SizedBox(height: AppSpacing.md),

              // ── Notes field (optional) ─────────────────────────────────
              _FieldLabel(label: l10n.notesOptional),
              const SizedBox(height: AppSpacing.xs),
              TextFormField(
                controller: _notesController,
                focusNode: _notesFocusNode,
                maxLines: 4,
                textInputAction: TextInputAction.done,
                decoration: _fieldDecoration(
                  hint: l10n.notesHint,
                ).copyWith(alignLabelWithHint: true),
              ),

              // ── Inline error ────────────────────────────────────────────
              AnimatedSize(
                duration: const Duration(milliseconds: 200),
                child: _errorMessage != null
                    ? Padding(
                        padding: const EdgeInsets.only(top: AppSpacing.sm),
                        child: Row(
                          children: [
                            const Icon(
                              Icons.error_outline_rounded,
                              color: AppColors.destructive,
                              size: 14,
                            ),
                            const SizedBox(width: 6),
                            Text(
                              _errorMessage!,
                              style: AppTextStyles.footnote.copyWith(
                                color: AppColors.destructive,
                              ),
                            ),
                          ],
                        ),
                      )
                    : const SizedBox.shrink(),
              ),

              const SizedBox(height: AppSpacing.xl),

              // ── Submit button ──────────────────────────────────────────
              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _submit,
                  child: _isLoading
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(
                            color: Colors.white,
                            strokeWidth: 2,
                          ),
                        )
                      : Text(l10n.requestAppointment),
                ),
              ),

              const SizedBox(height: AppSpacing.md),

              // ── Info note ──────────────────────────────────────────────
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Icon(
                    Icons.info_outline_rounded,
                    size: 14,
                    color: AppColors.textSecondary,
                  ),
                  const SizedBox(width: AppSpacing.xs),
                  Expanded(
                    child: Text(
                      l10n.therapistWillConfirm,
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  InputDecoration _fieldDecoration({String? hint, IconData? icon}) {
    return InputDecoration(
      hintText: hint,
      hintStyle: AppTextStyles.body.copyWith(color: AppColors.textSecondary),
      filled: true,
      fillColor: AppColors.surface,
      prefixIcon: icon != null
          ? Icon(icon, color: AppColors.textSecondary, size: 20)
          : null,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide.none,
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide.none,
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppColors.primary, width: 1.5),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppColors.destructive),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppColors.destructive, width: 1.5),
      ),
    );
  }
}

class _FieldLabel extends StatelessWidget {
  const _FieldLabel({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    return Text(
      label,
      style: AppTextStyles.footnote.copyWith(
        color: AppColors.textSecondary,
        fontWeight: FontWeight.w600,
      ),
    );
  }
}
