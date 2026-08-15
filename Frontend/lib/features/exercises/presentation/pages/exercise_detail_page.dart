import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:video_player/video_player.dart';
import 'package:youtube_player_iframe/youtube_player_iframe.dart';
import 'package:practice/core/network/api_config.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_spacing.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/core/widgets/feedback_chips.dart';
import 'package:practice/features/exercises/domain/entities/exercise_assignment.dart';
import 'package:practice/features/exercises/presentation/providers/exercise_fetch_provider.dart';
import 'package:practice/features/exercises/presentation/providers/providers.dart';
import 'package:practice/l10n/app_localizations.dart';

class ExerciseDetailPage extends ConsumerStatefulWidget {
  const ExerciseDetailPage({super.key, required this.assignment});

  final ExerciseAssignment assignment;

  @override
  ConsumerState<ExerciseDetailPage> createState() => _ExerciseDetailPageState();
}

class _ExerciseDetailPageState extends ConsumerState<ExerciseDetailPage> {
  late int? _feedback;
  DateTime? _completedAt;

  @override
  void initState() {
    super.initState();
    _feedback = widget.assignment.feedback;
    _completedAt = widget.assignment.completedAt;
  }

  ExerciseAssignment get assignment => widget.assignment;

  String _difficultyLabel(AppLocalizations l10n) =>
      switch (assignment.difficulty) {
        DifficultyLevel.easy => l10n.difficultyEasy,
        DifficultyLevel.moderate => l10n.difficultyModerate,
        DifficultyLevel.hard => l10n.difficultyHard,
      };

  Color get _difficultyColor => switch (assignment.difficulty) {
        DifficultyLevel.easy => AppColors.secondary,
        DifficultyLevel.moderate => const Color(0xFFF59E0B),
        DifficultyLevel.hard => AppColors.destructive,
      };

  Color get _difficultyBg => switch (assignment.difficulty) {
        DifficultyLevel.easy => AppColors.secondaryLight,
        DifficultyLevel.moderate => const Color(0xFFFFF8E6),
        DifficultyLevel.hard => const Color(0xFFFFEEEE),
      };

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Column(
        children: [
          // â”€â”€ Dark video hero â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          _VideoHero(
            videoUrl: assignment.videoUrl,
            onBack: () => Navigator.pop(context),
          ),

          // â”€â”€ Scrollable content â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
          Expanded(
            child: SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Title section
                  Container(
                    color: AppColors.surface,
                    padding: const EdgeInsets.fromLTRB(
                      AppSpacing.md,
                      AppSpacing.md,
                      AppSpacing.md,
                      AppSpacing.md,
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Difficulty + breadcrumb
                        Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 10,
                                vertical: 4,
                              ),
                              decoration: BoxDecoration(
                                color: _difficultyBg,
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Text(
                                _difficultyLabel(l10n),
                                style: AppTextStyles.caption.copyWith(
                                  color: _difficultyColor,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                            ),
                          ],
                        ),

                        const SizedBox(height: 10),

                        // Exercise name
                        Text(
                          assignment.exerciseName,
                          style: AppTextStyles.title1.copyWith(
                            color: AppColors.textHeading,
                            fontWeight: FontWeight.w800,
                            letterSpacing: -0.5,
                          ),
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: AppSpacing.sm),

                  // â”€â”€ Stats row â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                  Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.md,
                    ),
                    child: Container(
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      decoration: BoxDecoration(
                        color: AppColors.surface,
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: Row(
                        children: [
                          _StatTile(
                            icon: Icons.refresh_rounded,
                            value: '${assignment.sets}',
                            label: l10n.statSets,
                          ),
                          _VertDivider(),
                          _StatTile(
                            icon: Icons.repeat_rounded,
                            value: '${assignment.reps}',
                            label: l10n.statReps,
                          ),
                          _VertDivider(),
                          _StatTile(
                            icon: Icons.timer_outlined,
                            value: '${assignment.durationMinutes}m',
                            label: l10n.statDuration,
                          ),
                        ],
                      ),
                    ),
                  ),

                  const SizedBox(height: AppSpacing.sm),

                  // â”€â”€ Instructions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                  Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.md,
                    ),
                    child: Container(
                      padding: const EdgeInsets.all(AppSpacing.md),
                      decoration: BoxDecoration(
                        color: AppColors.surface,
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            l10n.instructions,
                            style: AppTextStyles.headline.copyWith(
                              color: AppColors.textPrimary,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                          const SizedBox(height: 12),
                          _buildInstructions(context),
                        ],
                      ),
                    ),
                  ),

                  const SizedBox(height: AppSpacing.sm),

                  // â”€â”€ Feedback section â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                  Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.md,
                    ),
                    child: _FeedbackCard(
                      feedback: _feedback,
                      completedAt: _completedAt,
                      onTap: () => _showFeedbackSheet(context),
                    ),
                  ),

                  const SizedBox(height: AppSpacing.xxxl),
                ],
              ),
            ),
          ),
        ],
      ),

      // â”€â”€ Bottom Button â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(
            AppSpacing.md,
            AppSpacing.sm,
            AppSpacing.md,
            AppSpacing.md,
          ),
          child: GestureDetector(
            onTap: () => _showFeedbackSheet(context),
            child: Container(
              width: double.infinity,
              height: 52,
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: _feedback == null
                      ? [AppColors.secondary, const Color(0xFF3A9E72)]
                      : [AppColors.primary, const Color(0xFF0062D9)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(AppRadius.pill),
                boxShadow: [
                  BoxShadow(
                    color: (_feedback == null ? AppColors.secondary : AppColors.primary)
                        .withValues(alpha: 0.30),
                    blurRadius: 14,
                    offset: const Offset(0, 5),
                  ),
                ],
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    _feedback == null
                        ? Icons.check_rounded
                        : Icons.edit_rounded,
                    color: Colors.white,
                    size: 18,
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Text(
                    _feedback == null ? l10n.markComplete : l10n.updateFeedback,
                    style: AppTextStyles.callout.copyWith(
                      color: Colors.white,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 0.2,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildInstructions(BuildContext context) {
    // Arabic steps when the app is in Arabic and a translation exists,
    // otherwise the English description.
    final arabic = assignment.descriptionAr;
    final useArabic = Localizations.localeOf(context).languageCode == 'ar' &&
        arabic != null &&
        arabic.trim().isNotEmpty;
    final description = useArabic ? arabic : assignment.description;

    // Arabic sentences end in the same punctuation, but also the Arabic
    // full stop (؟ / ۔) — split on either so steps still numbers correctly.
    final sentences = description
        .split(RegExp(r'(?<=[.!?؟۔])\s+'))
        .where((s) => s.trim().isNotEmpty)
        .toList();

    final textDirection = useArabic ? TextDirection.rtl : TextDirection.ltr;

    if (sentences.length <= 1) {
      return Text(
        description,
        textDirection: textDirection,
        style: AppTextStyles.body.copyWith(
          color: AppColors.textSecondary,
          height: 1.6,
        ),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: sentences.asMap().entries.map((entry) {
        return Padding(
          padding: const EdgeInsets.only(bottom: 10),
          child: Directionality(
            textDirection: textDirection,
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  width: 22,
                  height: 22,
                  decoration: BoxDecoration(
                    color: AppColors.primaryLight,
                    shape: BoxShape.circle,
                  ),
                  child: Center(
                    child: Text(
                      '${entry.key + 1}',
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.primary,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: Text(
                    entry.value.trim(),
                    style: AppTextStyles.body.copyWith(
                      color: AppColors.textSecondary,
                      height: 1.5,
                    ),
                  ),
                ),
              ],
            ),
          ),
        );
      }).toList(),
    );
  }

  void _showFeedbackSheet(BuildContext context) {
    int? selected = _feedback;
    final l10n = AppLocalizations.of(context)!;

    showModalBottomSheet(
      context: context,
      backgroundColor: AppColors.background,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (_) {
        return StatefulBuilder(
          builder: (context, setSheetState) {
            return Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Container(
                width: double.infinity,
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  color: AppColors.surface,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(l10n.howDidThisFeel, style: AppTextStyles.headline),
                    const SizedBox(height: AppSpacing.xs),
                    Text(
                      l10n.feedbackHelpsTherapist,
                      style: AppTextStyles.subheadline.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                    const SizedBox(height: AppSpacing.md),
                    FeedbackChips(
                      selected: selected,
                      onChanged: (level) {
                        setSheetState(() => selected = level);
                      },
                    ),
                    const SizedBox(height: AppSpacing.lg),
                    SizedBox(
                      width: double.infinity,
                      height: 48,
                      child: ElevatedButton(
                        onPressed: selected == null
                            ? null
                            : () async {
                                setState(() {
                                  _feedback = selected;
                                  _completedAt = DateTime.now();
                                });
                                final repo = ref.read(exerciseRepositoryProvider);
                                await repo.submitFeedback(
                                  assignment.exerciseAssignmentId,
                                  selected!,
                                );
                                ref.invalidate(exercisesProvider);
                                if (context.mounted) Navigator.pop(context);
                              },
                        child: Text(l10n.submitFeedback),
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }
}

// â”€â”€ Private Widgets â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

class _FeedbackCard extends StatelessWidget {
  const _FeedbackCard({
    required this.feedback,
    required this.completedAt,
    required this.onTap,
  });

  final int? feedback;
  final DateTime? completedAt;
  final VoidCallback onTap;

  static const _months = [
    'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
  ];

  static String _formatDate(AppLocalizations l10n, DateTime date) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final day = DateTime(date.year, date.month, date.day);
    final diff = today.difference(day).inDays;
    if (diff == 0) return l10n.feedbackLoggedToday;
    if (diff == 1) return l10n.feedbackLoggedYesterday;
    return '${date.day} ${_months[date.month - 1]}';
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final rating = feedback;
    return Semantics(
      button: true,
      label: rating == null
          ? l10n.rateHowFeltA11y
          : l10n.effortRatedA11y(FeedbackChips.labelFor(l10n, rating), rating),
      child: GestureDetector(
        onTap: onTap,
        child: Container(
          width: double.infinity,
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            color: AppColors.surface,
            borderRadius: BorderRadius.circular(AppRadius.card),
          ),
          child: rating == null
              ? _buildPrompt(l10n)
              : _buildSubmitted(l10n, rating),
        ),
      ),
    );
  }

  Widget _buildPrompt(AppLocalizations l10n) {
    return Row(
      children: [
        Container(
          width: 44,
          height: 44,
          decoration: const BoxDecoration(
            color: AppColors.primaryLight,
            shape: BoxShape.circle,
          ),
          child: const Icon(
            Icons.rate_review_outlined,
            color: AppColors.primary,
            size: 22,
          ),
        ),
        const SizedBox(width: AppSpacing.md),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                l10n.howDidThisFeel,
                style: AppTextStyles.headline.copyWith(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                l10n.rateEffortPrompt,
                style: AppTextStyles.subheadline.copyWith(
                  color: AppColors.textSecondary,
                ),
              ),
            ],
          ),
        ),
        const SizedBox(width: AppSpacing.sm),
        const Icon(
          Icons.chevron_right_rounded,
          color: AppColors.textSecondary,
          size: 22,
        ),
      ],
    );
  }

  Widget _buildSubmitted(AppLocalizations l10n, int rating) {
    final color = FeedbackChips.colorFor(rating);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.12),
                shape: BoxShape.circle,
              ),
              child: Icon(Icons.check_rounded, color: color, size: 22),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    FeedbackChips.labelFor(l10n, rating),
                    style: AppTextStyles.headline.copyWith(
                      color: AppColors.textPrimary,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    completedAt != null
                        ? l10n.feedbackLoggedOn(_formatDate(l10n, completedAt!))
                        : l10n.feedbackLogged,
                    style: AppTextStyles.footnote.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(width: AppSpacing.sm),
            Text(
              l10n.edit,
              style: AppTextStyles.callout.copyWith(
                color: AppColors.primary,
                fontWeight: FontWeight.w600,
              ),
            ),
            const Icon(
              Icons.chevron_right_rounded,
              color: AppColors.primary,
              size: 18,
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.md),
        _IntensityBar(rating: rating),
        const SizedBox(height: 6),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(l10n.veryEasyLabel, style: AppTextStyles.caption),
            Text(
              l10n.ratingOutOfTen(rating),
              style: AppTextStyles.caption.copyWith(
                color: color,
                fontWeight: FontWeight.w600,
              ),
            ),
            Text(l10n.maxEffortLabel, style: AppTextStyles.caption),
          ],
        ),
      ],
    );
  }
}

class _IntensityBar extends StatelessWidget {
  const _IntensityBar({required this.rating});

  final int rating;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        for (int i = 1; i <= 10; i++) ...[
          if (i > 1) const SizedBox(width: 2),
          Expanded(
            child: Container(
              height: 4,
              decoration: BoxDecoration(
                color: FeedbackChips.colorFor(i).withValues(
                  alpha: i <= rating ? 1.0 : 0.18,
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

/// Extracts a YouTube video id from a bare id, `youtu.be/...`, or
/// `youtube.com/(watch?v=|embed/|shorts/)...` URL. Returns null for anything
/// else (direct media file URLs, server-relative paths, empty/null input).
String? _extractYouTubeId(String? url) {
  if (url == null || url.trim().isEmpty) return null;
  final match = RegExp(
    r'(?:youtu\.be/|youtube\.com/(?:embed/|watch\?v=|shorts/))([^?&#/]+)',
    caseSensitive: false,
  ).firstMatch(url);
  return match?.group(1);
}

class _VideoHero extends StatelessWidget {
  const _VideoHero({required this.videoUrl, required this.onBack});

  /// URL of the assigned exercise's video. May be a full URL, a server-relative
  /// path (e.g. `/images/clip.mov`), a YouTube link, or null when no video is
  /// assigned.
  final String? videoUrl;
  final VoidCallback onBack;

  @override
  Widget build(BuildContext context) {
    final youtubeId = _extractYouTubeId(videoUrl);
    return youtubeId != null
        ? _YoutubeVideoHero(videoId: youtubeId, onBack: onBack)
        : _DirectVideoHero(videoUrl: videoUrl, onBack: onBack);
  }
}

class _YoutubeVideoHero extends StatefulWidget {
  const _YoutubeVideoHero({required this.videoId, required this.onBack});

  final String videoId;
  final VoidCallback onBack;

  @override
  State<_YoutubeVideoHero> createState() => _YoutubeVideoHeroState();
}

class _YoutubeVideoHeroState extends State<_YoutubeVideoHero> {
  // Native controls are kept off — YouTube always draws its own title/channel
  // overlay as part of that control bar, and there's no param to suppress just
  // the title. Tap-to-play/pause below replaces the native controls instead.
  late final YoutubePlayerController _controller =
      YoutubePlayerController.fromVideoId(
    videoId: widget.videoId,
    autoPlay: false,
    params: const YoutubePlayerParams(
      showControls: false,
      showFullscreenButton: false,
      strictRelatedVideos: true,
    ),
  );

  @override
  void dispose() {
    _controller.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 240,
      child: Stack(
        fit: StackFit.expand,
        children: [
          ColoredBox(
            color: const Color(0xFF0D1B2E),
            child: YoutubePlayer(controller: _controller),
          ),

          // Tap to play / pause
          GestureDetector(
            behavior: HitTestBehavior.opaque,
            onTap: () {
              final playing =
                  _controller.value.playerState == PlayerState.playing;
              playing ? _controller.pauseVideo() : _controller.playVideo();
            },
            child: YoutubeValueBuilder(
              controller: _controller,
              builder: (context, value) {
                final playing = value.playerState == PlayerState.playing;
                return Center(
                  child: AnimatedOpacity(
                    opacity: playing ? 0.0 : 1.0,
                    duration: const Duration(milliseconds: 200),
                    child: Container(
                      width: 56,
                      height: 56,
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.18),
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: Colors.white.withValues(alpha: 0.4),
                          width: 1.5,
                        ),
                      ),
                      child: const Icon(
                        Icons.play_arrow_rounded,
                        color: Colors.white,
                        size: 30,
                      ),
                    ),
                  ),
                );
              },
            ),
          ),

          // Back button
          Positioned(
            top: MediaQuery.of(context).padding.top + 8,
            left: 12,
            child: GestureDetector(
              onTap: widget.onBack,
              child: Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: Colors.black.withValues(alpha: 0.35),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.arrow_back_rounded,
                  color: Colors.white,
                  size: 20,
                ),
              ),
            ),
          ),

          // Fullscreen toggle — the package handles rotation, the fullscreen
          // overlay, and exiting on system back once entered.
          Positioned(
            top: MediaQuery.of(context).padding.top + 8,
            right: 12,
            child: GestureDetector(
              onTap: () => _controller.enterFullScreen(),
              child: Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: Colors.black.withValues(alpha: 0.35),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.fullscreen_rounded,
                  color: Colors.white,
                  size: 22,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _DirectVideoHero extends StatefulWidget {
  const _DirectVideoHero({required this.videoUrl, required this.onBack});

  /// URL of a direct media file (server-relative path or full URL), or null
  /// when no video is assigned.
  final String? videoUrl;
  final VoidCallback onBack;

  @override
  State<_DirectVideoHero> createState() => _DirectVideoHeroState();
}

class _DirectVideoHeroState extends State<_DirectVideoHero> {
  late final VideoPlayerController _controller;
  bool _initialized = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _controller = _buildController();
    _controller.initialize().then((_) {
      if (mounted) setState(() => _initialized = true);
    }).catchError((e) {
      if (mounted) setState(() => _error = e.toString());
    });
  }

  VideoPlayerController _buildController() {
    final url = widget.videoUrl;
    if (url == null || url.isEmpty) {
      // No video assigned — fall back to the bundled demo clip.
      return VideoPlayerController.asset('assets/videos/cat_cow_stretch.mp4');
    }
    // Server-relative paths (e.g. /images/foo.mov) resolve against the API host.
    final resolved = url.startsWith('http') ? url : '$kApiBaseUrl$url';
    return VideoPlayerController.networkUrl(Uri.parse(resolved));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  String _formatDuration(Duration d) {
    final m = d.inMinutes.remainder(60).toString().padLeft(1, '0');
    final s = d.inSeconds.remainder(60).toString().padLeft(2, '0');
    return '$m:$s';
  }

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 240,
      child: Stack(
        fit: StackFit.expand,
        children: [
          // Video or dark background while loading
          ColoredBox(
            color: const Color(0xFF0D1B2E),
            child: _error != null
                ? Center(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.movie_outlined,
                            color: Colors.white24, size: 48),
                        const SizedBox(height: 8),
                        Text(
                          AppLocalizations.of(context)!.videoPreviewUnavailable,
                          style: const TextStyle(
                              color: Colors.white54, fontSize: 12),
                        ),
                      ],
                    ),
                  )
                : _initialized
                    ? FittedBox(
                        fit: BoxFit.cover,
                        child: SizedBox(
                          width: _controller.value.size.width,
                          height: _controller.value.size.height,
                          child: VideoPlayer(_controller),
                        ),
                      )
                    : const Center(
                        child: CircularProgressIndicator(color: Colors.white38),
                      ),
          ),

          // Tap to play / pause
          GestureDetector(
            behavior: HitTestBehavior.opaque,
            onTap: () {
              if (!_initialized) return;
              setState(() {
                _controller.value.isPlaying
                    ? _controller.pause()
                    : _controller.play();
              });
            },
            child: AnimatedBuilder(
              animation: _controller,
              builder: (_, _) {
                final playing = _controller.value.isPlaying;
                return Center(
                  child: AnimatedOpacity(
                    opacity: playing ? 0.0 : 1.0,
                    duration: const Duration(milliseconds: 200),
                    child: Container(
                      width: 56,
                      height: 56,
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.18),
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: Colors.white.withValues(alpha: 0.4),
                          width: 1.5,
                        ),
                      ),
                      child: const Icon(
                        Icons.play_arrow_rounded,
                        color: Colors.white,
                        size: 30,
                      ),
                    ),
                  ),
                );
              },
            ),
          ),

          // Duration badge
          Positioned(
            bottom: 12,
            right: 12,
            child: AnimatedBuilder(
              animation: _controller,
              builder: (_, _) {
                final duration = _initialized
                    ? _controller.value.duration
                    : Duration.zero;
                return Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.black.withValues(alpha: 0.5),
                    borderRadius: BorderRadius.circular(6),
                  ),
                  child: Text(
                    _formatDuration(duration),
                    style: AppTextStyles.caption.copyWith(
                      color: Colors.white,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                );
              },
            ),
          ),

          // Back button
          Positioned(
            top: MediaQuery.of(context).padding.top + 8,
            left: 12,
            child: GestureDetector(
              onTap: widget.onBack,
              child: Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.arrow_back_rounded,
                  color: Colors.white,
                  size: 20,
                ),
              ),
            ),
          ),

          // Fullscreen toggle — pushes a landscape page sharing this same
          // controller, so playback position/state carries over seamlessly.
          Positioned(
            top: MediaQuery.of(context).padding.top + 8,
            right: 12,
            child: GestureDetector(
              onTap: _initialized
                  ? () => Navigator.of(context).push(
                        MaterialPageRoute(
                          fullscreenDialog: true,
                          builder: (_) =>
                              _FullscreenDirectVideoPage(controller: _controller),
                        ),
                      )
                  : null,
              child: Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.fullscreen_rounded,
                  color: Colors.white,
                  size: 22,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _FullscreenDirectVideoPage extends StatefulWidget {
  const _FullscreenDirectVideoPage({required this.controller});

  final VideoPlayerController controller;

  @override
  State<_FullscreenDirectVideoPage> createState() =>
      _FullscreenDirectVideoPageState();
}

class _FullscreenDirectVideoPageState
    extends State<_FullscreenDirectVideoPage> with WidgetsBindingObserver {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    SystemChrome.setPreferredOrientations([
      DeviceOrientation.landscapeLeft,
      DeviceOrientation.landscapeRight,
    ]);
    _hideSystemBars();
  }

  // The orientation change above triggers a metrics/insets reset on many
  // Android versions, which silently brings the system bars back — re-hide
  // them on every such change for as long as this page is on screen.
  @override
  void didChangeMetrics() => _hideSystemBars();

  void _hideSystemBars() {
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersiveSticky);
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final controller = widget.controller;
    return Scaffold(
      backgroundColor: Colors.black,
      body: Stack(
        fit: StackFit.expand,
        children: [
          Center(
            child: AspectRatio(
              aspectRatio: controller.value.aspectRatio,
              child: VideoPlayer(controller),
            ),
          ),

          // Tap to play / pause
          GestureDetector(
            behavior: HitTestBehavior.opaque,
            onTap: () {
              setState(() {
                controller.value.isPlaying
                    ? controller.pause()
                    : controller.play();
              });
            },
            child: AnimatedBuilder(
              animation: controller,
              builder: (_, _) {
                final playing = controller.value.isPlaying;
                return Center(
                  child: AnimatedOpacity(
                    opacity: playing ? 0.0 : 1.0,
                    duration: const Duration(milliseconds: 200),
                    child: Container(
                      width: 64,
                      height: 64,
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.18),
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: Colors.white.withValues(alpha: 0.4),
                          width: 1.5,
                        ),
                      ),
                      child: const Icon(
                        Icons.play_arrow_rounded,
                        color: Colors.white,
                        size: 36,
                      ),
                    ),
                  ),
                );
              },
            ),
          ),

          // Exit fullscreen
          Positioned(
            top: 16,
            left: 16,
            child: GestureDetector(
              onTap: () => Navigator.of(context).pop(),
              child: Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.fullscreen_exit_rounded,
                  color: Colors.white,
                  size: 22,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _StatTile extends StatelessWidget {
  const _StatTile({
    required this.icon,
    required this.value,
    required this.label,
  });

  final IconData icon;
  final String value;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Column(
        children: [
          Icon(icon, size: 18, color: AppColors.primary),
          const SizedBox(height: 6),
          Text(
            value,
            style: AppTextStyles.title2.copyWith(
              color: AppColors.textPrimary,
              fontWeight: FontWeight.w800,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            style: AppTextStyles.caption.copyWith(
              color: AppColors.textSecondary,
              letterSpacing: 0.4,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

class _VertDivider extends StatelessWidget {
  @override
  Widget build(BuildContext context) =>
      Container(width: 1, height: 36, color: AppColors.divider);
}
