import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_svg/svg.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';
import 'package:practice/l10n/app_localizations.dart';

class LoginPage extends ConsumerStatefulWidget {
  const LoginPage({super.key});

  @override
  ConsumerState<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends ConsumerState<LoginPage> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _passwordFocusNode = FocusNode();
  final _formKey = GlobalKey<FormState>();

  bool _isLoading = false;
  bool _obscurePassword = true;
  String? _errorMessage;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _passwordFocusNode.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _errorMessage = null);
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);
    try {
      await ref
          .read(authNotifierProvider)
          .login(_emailController.text.trim(), _passwordController.text);
    } catch (ex) {
      if (!mounted) return;
      setState(
        () => _errorMessage = AppLocalizations.of(context)!.incorrectCredentials,
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
              children: [
                const SizedBox(height: 56),

                // Logo icon
                SvgPicture.asset(
                  'assets/images/physiolink_logo.svg',
                  width: 52,
                  height: 52,
                ),

                const SizedBox(height: 14),

                // App name
                Text(
                  'PhysioLink',
                  style: AppTextStyles.largeTitle.copyWith(
                    color: AppColors.primary,
                    fontWeight: FontWeight.w800,
                    letterSpacing: -0.5,
                  ),
                ),

                const SizedBox(height: 6),

                Text(
                  l10n.tagline,
                  textAlign: TextAlign.center,
                  style: AppTextStyles.callout.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),

                const SizedBox(height: AppSpacing.xxl),

                // Floating form card
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
                        // â”€â”€ Email â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                        Text(
                          l10n.emailLabel,
                          style: AppTextStyles.footnote.copyWith(
                            color: AppColors.textSecondary,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(height: 6),
                        TextFormField(
                          controller: _emailController,
                          keyboardType: TextInputType.emailAddress,
                          textInputAction: TextInputAction.next,
                          autocorrect: false,
                          onFieldSubmitted: (_) => FocusScope.of(
                            context,
                          ).requestFocus(_passwordFocusNode),
                          validator: (v) => (v == null || v.isEmpty)
                              ? l10n.emailRequired
                              : null,
                          decoration: InputDecoration(
                            hintText: 'you@example.com',
                            hintStyle: AppTextStyles.body.copyWith(
                              color: AppColors.textSecondary,
                            ),
                            prefixIcon: const Icon(
                              Icons.mail_outline_rounded,
                              size: 18,
                              color: AppColors.textSecondary,
                            ),
                          ),
                        ),

                        const SizedBox(height: AppSpacing.md),

                        // â”€â”€ Password â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                        Text(
                          l10n.password,
                          style: AppTextStyles.footnote.copyWith(
                            color: AppColors.textSecondary,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(height: 6),
                        TextFormField(
                          controller: _passwordController,
                          focusNode: _passwordFocusNode,
                          obscureText: _obscurePassword,
                          textInputAction: TextInputAction.done,
                          onFieldSubmitted: (_) => _submit(),
                          validator: (v) {
                            if (v == null || v.isEmpty) {
                              return l10n.passwordRequired;
                            }
                            if (v.length < 6) {
                              return l10n.passwordMinLength;
                            }
                            return null;
                          },
                          decoration: InputDecoration(
                            hintText: l10n.passwordHint,
                            hintStyle: AppTextStyles.body.copyWith(
                              color: AppColors.textSecondary,
                            ),
                            prefixIcon: const Icon(
                              Icons.lock_outline_rounded,
                              size: 18,
                              color: AppColors.textSecondary,
                            ),
                            suffixIcon: GestureDetector(
                              onTap: () => setState(
                                () => _obscurePassword = !_obscurePassword,
                              ),
                              child: Padding(
                                padding: const EdgeInsetsDirectional.only(
                                  end: 14,
                                ),
                                child: Align(
                                  widthFactor: 1,
                                  child: Text(
                                    _obscurePassword ? l10n.show : l10n.hide,
                                    style: AppTextStyles.footnote.copyWith(
                                      color: AppColors.primary,
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                ),
                              ),
                            ),
                          ),
                        ),

                        // â”€â”€ Error â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

                        const SizedBox(height: AppSpacing.lg),

                        // â”€â”€ Log In Button â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
                              borderRadius: BorderRadius.circular(
                                AppRadius.pill,
                              ),
                              boxShadow: _isLoading
                                  ? null
                                  : [
                                      BoxShadow(
                                        color: AppColors.secondary.withValues(alpha: 
                                          0.35,
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
                                      l10n.logIn,
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

                const SizedBox(height: AppSpacing.xxl),

                // Secured by text
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.lock_outline,
                      size: 11,
                      color: AppColors.textSecondary.withValues(alpha: 0.45),
                    ),
                    const SizedBox(width: 4),
                    Text(
                      l10n.securedByFooter,
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.textSecondary.withValues(alpha: 0.45),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: AppSpacing.lg),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
