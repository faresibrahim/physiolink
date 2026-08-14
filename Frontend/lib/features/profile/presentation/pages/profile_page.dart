import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/error/app_failure.dart';
import 'package:practice/core/localization/locale_controller.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/core/widgets/empty_state.dart';
import 'package:practice/core/widgets/shimmer_card.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';
import 'package:practice/features/profile/domain/entities/patient.dart';
import 'package:practice/features/profile/presentation/providers/profile_providers.dart';
import 'package:practice/l10n/app_localizations.dart';

class ProfilePage extends ConsumerWidget {
  const ProfilePage({super.key});

  String _initials(Patient p) {
    return '${p.firstName[0]}${p.lastName[0]}'.toUpperCase();
  }

  Future<void> _confirmLogout(BuildContext context, WidgetRef ref) async {
    final l10n = AppLocalizations.of(context)!;
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppColors.surface,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Text(l10n.logOutQuestion, style: AppTextStyles.headline),
        content: Text(
          l10n.logOutBody,
          style: AppTextStyles.body.copyWith(color: AppColors.textSecondary),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: Text(
              l10n.cancel,
              style: AppTextStyles.body.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
          ),
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(true),
            child: Text(
              l10n.logOut,
              style: AppTextStyles.body.copyWith(
                color: AppColors.destructive,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
    if (confirmed == true) {
      await ref.read(authNotifierProvider).logout();
    }
  }

  Future<void> _selectLanguage(BuildContext context, WidgetRef ref) async {
    final l10n = AppLocalizations.of(context)!;
    final controller = ref.read(localeControllerProvider.notifier);
    final current = ref.read(localeControllerProvider).languageCode;

    await showModalBottomSheet<void>(
      context: context,
      backgroundColor: AppColors.surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Text(l10n.language, style: AppTextStyles.headline),
            ),
            _LangTile(
              label: l10n.english,
              selected: current == 'en',
              onTap: () {
                controller.setLocale(const Locale('en'));
                Navigator.of(ctx).pop();
              },
            ),
            const Divider(height: 1, color: AppColors.divider),
            _LangTile(
              label: l10n.arabic,
              selected: current == 'ar',
              onTap: () {
                controller.setLocale(const Locale('ar'));
                Navigator.of(ctx).pop();
              },
            ),
            const SizedBox(height: AppSpacing.sm),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(profileProvider);
    final l10n = AppLocalizations.of(context)!;

    return SafeArea(
      child: CustomScrollView(
        slivers: [
          // ── Title ─────────────────────────────────────────────────────
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(
                AppSpacing.md,
                AppSpacing.md,
                AppSpacing.md,
                0,
              ),
              child: Text(
                l10n.profile,
                style: AppTextStyles.title1.copyWith(
                  color: AppColors.textHeading,
                  fontWeight: FontWeight.w800,
                  letterSpacing: -0.5,
                ),
              ),
            ),
          ),

          // ── Content ───────────────────────────────────────────────────
          profileAsync.when(
            loading: () => SliverPadding(
              padding: const EdgeInsets.all(AppSpacing.md),
              sliver: SliverList.separated(
                itemCount: 4,
                separatorBuilder: (_, _) =>
                    const SizedBox(height: AppSpacing.sm),
                itemBuilder: (_, _) => const ShimmerCard(height: 80),
              ),
            ),
            error: (_, _) => SliverFillRemaining(
              child: EmptyState(
                icon: Icons.wifi_off_rounded,
                title: l10n.somethingWentWrong,
                subtitle: l10n.pullToRetry,
                ctaLabel: l10n.retry,
                onCta: () => ref.invalidate(profileProvider),
              ),
            ),
            data: (result) => result.fold(
              (failure) => SliverFillRemaining(
                child: _buildError(context, failure, ref),
              ),
              (patient) => _buildProfile(context, ref, patient),
            ),
          ),

          const SliverToBoxAdapter(child: SizedBox(height: AppSpacing.xl)),
        ],
      ),
    );
  }

  Widget _buildError(BuildContext context, AppFailure failure, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final msg = switch (failure) {
      NetworkFailure() => l10n.noInternet,
      ServerFailure() => l10n.serverError,
      AuthFailure() => l10n.sessionExpired,
    };
    return EmptyState(
      icon: Icons.error_outline_rounded,
      title: l10n.couldntLoadProfile,
      subtitle: msg,
      ctaLabel: l10n.retry,
      onCta: () => ref.invalidate(profileProvider),
    );
  }

  Widget _buildProfile(BuildContext context, WidgetRef ref, Patient patient) {
    final l10n = AppLocalizations.of(context)!;
    final currentLang = ref.watch(localeControllerProvider).languageCode;
    return SliverPadding(
      padding: const EdgeInsets.all(AppSpacing.md),
      sliver: SliverList(
        delegate: SliverChildListDelegate([
          // ── Identity card ─────────────────────────────────────────────
          Container(
            padding: const EdgeInsets.all(AppSpacing.md),
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              children: [
                // Avatar
                Container(
                  width: 48,
                  height: 48,
                  decoration: const BoxDecoration(
                    color: AppColors.primary,
                    shape: BoxShape.circle,
                  ),
                  child: Center(
                    child: Text(
                      _initials(patient),
                      style: AppTextStyles.headline.copyWith(
                        color: Colors.white,
                        fontWeight: FontWeight.w700,
                        fontSize: 18,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        '${patient.firstName} ${patient.lastName}',
                        style: AppTextStyles.headline.copyWith(
                          color: AppColors.textPrimary,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 3),
                      Row(
                        children: [
                          const Icon(
                            Icons.healing_outlined,
                            size: 12,
                            color: AppColors.textSecondary,
                          ),
                          const SizedBox(width: 3),
                          Expanded(
                            child: Text(
                              patient.diagnosis,
                              style: AppTextStyles.caption.copyWith(
                                color: AppColors.textSecondary,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: AppSpacing.sm),

          // ── Personal info ─────────────────────────────────────────────
          _SectionLabel(label: l10n.personalInfo),
          const SizedBox(height: AppSpacing.sm),

          Container(
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              children: [
                _InfoRow(
                  icon: Icons.mail_outline_rounded,
                  label: l10n.email,
                  value: patient.email,
                  showDivider: true,
                ),
                _InfoRow(
                  icon: Icons.phone_outlined,
                  label: l10n.phone,
                  value: patient.phoneNumber,
                  showDivider: true,
                ),
                _InfoRow(
                  icon: Icons.local_hospital_outlined,
                  label: l10n.clinic,
                  value: patient.clinicName ?? l10n.unassigned,
                  showDivider: true,
                ),
                _InfoRow(
                  icon: Icons.medical_services_outlined,
                  label: l10n.therapist,
                  value: patient.therapistName ?? l10n.unassigned,
                  showDivider: false,
                ),
              ],
            ),
          ),

          const SizedBox(height: AppSpacing.lg),

          // Preferences
          _SectionLabel(label: l10n.preferences),
          const SizedBox(height: AppSpacing.sm),

          Container(
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              children: [
                _PreferenceRow(
                  icon: Icons.language_outlined,
                  label: l10n.language,
                  onTap: () => _selectLanguage(context, ref),
                  trailing: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        currentLang == 'ar' ? l10n.arabic : l10n.english,
                        style: AppTextStyles.footnote.copyWith(
                          color: AppColors.textSecondary,
                        ),
                      ),
                      const SizedBox(width: 4),
                      const Icon(
                        Icons.chevron_right_rounded,
                        size: 16,
                        color: AppColors.textSecondary,
                      ),
                    ],
                  ),
                  showDivider: false,
                ),
              ],
            ),
          ),

          const SizedBox(height: AppSpacing.xl),

          //Log out button
          SizedBox(
            width: double.infinity,
            height: 50,
            child: OutlinedButton(
              onPressed: () => _confirmLogout(context, ref),
              style: OutlinedButton.styleFrom(
                foregroundColor: AppColors.destructive,
                side: const BorderSide(color: AppColors.destructive, width: 1),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(AppRadius.pill),
                ),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.logout_rounded, size: 18),
                  const SizedBox(width: 8),
                  Text(
                    l10n.logOut,
                    style: AppTextStyles.callout.copyWith(
                      color: AppColors.destructive,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ),

          const SizedBox(height: AppSpacing.lg),

          //Version footer
          Center(
            child: Text(
              'PhysioLink · Version 2.4.0',
              style: AppTextStyles.caption.copyWith(
                color: AppColors.textSecondary.withValues(alpha: 0.5),
              ),
            ),
          ),
        ]),
      ),
    );
  }
}

//Private Widgets

class _SectionLabel extends StatelessWidget {
  const _SectionLabel({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    return Text(
      label,
      style: AppTextStyles.footnote.copyWith(
        color: AppColors.textSecondary,
        fontWeight: FontWeight.w600,
        letterSpacing: 0.2,
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({
    required this.icon,
    required this.label,
    required this.value,
    required this.showDivider,
  });

  final IconData icon;
  final String label;
  final String value;
  final bool showDivider;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: AppSpacing.md,
            vertical: 14,
          ),
          child: Row(
            children: [
              Container(
                width: 34,
                height: 34,
                decoration: BoxDecoration(
                  color: AppColors.background,
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Icon(icon, size: 16, color: AppColors.primary),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      label,
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.textSecondary,
                        letterSpacing: 0.4,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      value,
                      style: AppTextStyles.subheadline.copyWith(
                        color: AppColors.textPrimary,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
        if (showDivider)
          const Padding(
            padding: EdgeInsetsDirectional.only(start: 66),
            child: Divider(height: 1, color: AppColors.divider),
          ),
      ],
    );
  }
}

class _PreferenceRow extends StatelessWidget {
  const _PreferenceRow({
    required this.icon,
    required this.label,
    required this.trailing,
    required this.showDivider,
    this.onTap,
  });

  final IconData icon;
  final String label;
  final Widget trailing;
  final bool showDivider;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final row = Padding(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.md,
        vertical: 12,
      ),
      child: Row(
        children: [
          Container(
            width: 34,
            height: 34,
            decoration: BoxDecoration(
              color: AppColors.background,
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(icon, size: 16, color: AppColors.primary),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Text(
              label,
              style: AppTextStyles.subheadline.copyWith(
                color: AppColors.textPrimary,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          trailing,
        ],
      ),
    );
    return Column(
      children: [
        if (onTap != null) InkWell(onTap: onTap, child: row) else row,
        if (showDivider)
          const Padding(
            padding: EdgeInsetsDirectional.only(start: 66),
            child: Divider(height: 1, color: AppColors.divider),
          ),
      ],
    );
  }
}

class _LangTile extends StatelessWidget {
  const _LangTile({
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      title: Text(
        label,
        style: AppTextStyles.body.copyWith(color: AppColors.textPrimary),
      ),
      trailing: selected
          ? const Icon(Icons.check_rounded, color: AppColors.primary)
          : null,
      onTap: onTap,
    );
  }
}
