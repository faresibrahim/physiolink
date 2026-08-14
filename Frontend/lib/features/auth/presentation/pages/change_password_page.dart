import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';
import 'package:practice/l10n/app_localizations.dart';

class ChangePasswordPage extends ConsumerStatefulWidget {
  const ChangePasswordPage({super.key});

  @override
  ConsumerState<ChangePasswordPage> createState() => _ChangePasswordPageState();
}

class _ChangePasswordPageState extends ConsumerState<ChangePasswordPage> {
  final _newController = TextEditingController();
  final _confirmController = TextEditingController();
  final _confirmFocusNode = FocusNode();
  final _formKey = GlobalKey<FormState>();

  bool _isLoading = false;
  bool _obscure = true;
  String? _errorMessage;

  @override
  void dispose() {
    _newController.dispose();
    _confirmController.dispose();
    _confirmFocusNode.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _errorMessage = null);
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);
    try {
      await ref
          .read(authNotifierProvider)
          .changePassword(_newController.text);
      // On success the notifier clears mustChangePassword and notifies the router,
      // which redirects to /home automatically — no manual navigation needed.
    } catch (_) {
      if (!mounted) return;
      setState(
        () => _errorMessage = AppLocalizations.of(context)!.changePasswordError,
      );
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.center,
            colors: [Color(0xFFD8E8FF), Colors.white],
          ),
        ),
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SizedBox(height: 56),

                Icon(
                  Icons.lock_reset_rounded,
                  size: 52,
                  color: AppColors.primary,
                ),

                const SizedBox(height: 14),

                Text(
                  l10n.changePasswordTitle,
                  textAlign: TextAlign.center,
                  style: AppTextStyles.largeTitle.copyWith(
                    color: AppColors.primary,
                    fontWeight: FontWeight.w800,
                    letterSpacing: -0.5,
                  ),
                ),

                const SizedBox(height: 6),

                Text(
                  l10n.changePasswordSubtitle,
                  textAlign: TextAlign.center,
                  style: AppTextStyles.callout.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),

                const SizedBox(height: AppSpacing.xxl),

                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(20),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.08),
                        blurRadius: 28,
                        offset: const Offset(0, 10),
                      ),
                    ],
                  ),
                  padding: const EdgeInsets.all(AppSpacing.lg),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        _fieldLabel(l10n.newPasswordLabel),
                        const SizedBox(height: 6),
                        TextFormField(
                          controller: _newController,
                          obscureText: _obscure,
                          textInputAction: TextInputAction.next,
                          autocorrect: false,
                          enableSuggestions: false,
                          onFieldSubmitted: (_) => FocusScope.of(
                            context,
                          ).requestFocus(_confirmFocusNode),
                          validator: (v) {
                            if (v == null || v.isEmpty) {
                              return l10n.newPasswordRequired;
                            }
                            if (v.length < 6) {
                              return l10n.passwordMinLength;
                            }
                            return null;
                          },
                          decoration: _fieldDecoration(
                            l10n.newPasswordHint,
                            withToggle: true,
                            l10n: l10n,
                          ),
                        ),

                        const SizedBox(height: AppSpacing.md),

                        _fieldLabel(l10n.confirmPasswordLabel),
                        const SizedBox(height: 6),
                        TextFormField(
                          controller: _confirmController,
                          focusNode: _confirmFocusNode,
                          obscureText: _obscure,
                          textInputAction: TextInputAction.done,
                          autocorrect: false,
                          enableSuggestions: false,
                          onFieldSubmitted: (_) => _submit(),
                          validator: (v) {
                            if (v == null || v.isEmpty) {
                              return l10n.confirmPasswordRequired;
                            }
                            if (v != _newController.text) {
                              return l10n.passwordsDoNotMatch;
                            }
                            return null;
                          },
                          decoration: _fieldDecoration(l10n.confirmPasswordHint),
                        ),

                        AnimatedSize(
                          duration: const Duration(milliseconds: 200),
                          child: _errorMessage != null
                              ? Padding(
                                  padding: const EdgeInsets.only(
                                    top: AppSpacing.sm,
                                  ),
                                  child: Row(
                                    children: [
                                      const Icon(
                                        Icons.error_outline_rounded,
                                        color: AppColors.destructive,
                                        size: 14,
                                      ),
                                      const SizedBox(width: 6),
                                      Expanded(
                                        child: Text(
                                          _errorMessage!,
                                          style: AppTextStyles.footnote.copyWith(
                                            color: AppColors.destructive,
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                )
                              : const SizedBox.shrink(),
                        ),

                        const SizedBox(height: AppSpacing.lg),

                        GestureDetector(
                          onTap: _isLoading ? null : _submit,
                          child: AnimatedContainer(
                            duration: const Duration(milliseconds: 150),
                            width: double.infinity,
                            height: 52,
                            decoration: BoxDecoration(
                              gradient: _isLoading
                                  ? null
                                  : const LinearGradient(
                                      colors: [
                                        Color(0xFF4CAF82),
                                        Color(0xFF3A9E72),
                                      ],
                                      begin: Alignment.topLeft,
                                      end: Alignment.bottomRight,
                                    ),
                              color: _isLoading
                                  ? AppColors.secondary.withValues(alpha: 0.5)
                                  : null,
                              borderRadius: BorderRadius.circular(AppRadius.pill),
                              boxShadow: _isLoading
                                  ? null
                                  : [
                                      BoxShadow(
                                        color: AppColors.secondary.withValues(
                                          alpha: 0.35,
                                        ),
                                        blurRadius: 16,
                                        offset: const Offset(0, 6),
                                      ),
                                    ],
                            ),
                            child: Center(
                              child: _isLoading
                                  ? const SizedBox(
                                      width: 20,
                                      height: 20,
                                      child: CircularProgressIndicator(
                                        color: Colors.white,
                                        strokeWidth: 2,
                                      ),
                                    )
                                  : Text(
                                      l10n.setPasswordCta,
                                      style: AppTextStyles.callout.copyWith(
                                        color: Colors.white,
                                        fontWeight: FontWeight.w700,
                                        letterSpacing: 0.2,
                                      ),
                                    ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),

                const SizedBox(height: AppSpacing.lg),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _fieldLabel(String text) => Text(
    text,
    style: AppTextStyles.footnote.copyWith(
      color: AppColors.textSecondary,
      fontWeight: FontWeight.w500,
    ),
  );

  InputDecoration _fieldDecoration(
    String hint, {
    bool withToggle = false,
    AppLocalizations? l10n,
  }) {
    return InputDecoration(
      hintText: hint,
      hintStyle: AppTextStyles.body.copyWith(color: AppColors.textSecondary),
      prefixIcon: const Icon(
        Icons.lock_outline_rounded,
        size: 18,
        color: AppColors.textSecondary,
      ),
      suffixIcon: withToggle && l10n != null
          ? GestureDetector(
              onTap: () => setState(() => _obscure = !_obscure),
              child: Padding(
                padding: const EdgeInsetsDirectional.only(end: 14),
                child: Align(
                  widthFactor: 1,
                  child: Text(
                    _obscure ? l10n.show : l10n.hide,
                    style: AppTextStyles.footnote.copyWith(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ),
            )
          : null,
    );
  }
}
