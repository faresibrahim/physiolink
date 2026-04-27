import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_svg/svg.dart';
import 'package:go_router/go_router.dart';
import 'package:practice/core/theme/app_colors.dart';
import 'package:practice/core/theme/app_text_styles.dart';
import 'package:practice/features/auth/presentation/providers/auth_provider.dart';

class SplashPage extends ConsumerStatefulWidget {
  const SplashPage({super.key});

  @override
  ConsumerState<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends ConsumerState<SplashPage>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _fade;
  late final Animation<double> _ecgProgress;

  @override
  void initState() {
    super.initState();

    _controller = AnimationController(
      duration: const Duration(milliseconds: 1800),
      vsync: this,
    );

    _fade = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.0, 0.35, curve: Curves.easeOut),
    );

    _ecgProgress = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.3, 0.85, curve: Curves.easeInOut),
    );

    _controller.forward();

    // Run session restore in parallel with the animation.
    // By the time the 2400ms delay completes, the token check
    // (or silent refresh) will already have finished.
    ref.read(authNotifierProvider).tryRestoreSession();

    Future.delayed(const Duration(milliseconds: 2400), () {
      if (!mounted) return;
      final auth = ref.read(authNotifierProvider);
      context.go(auth.isLoggedIn ? '/home' : '/');
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: FadeTransition(
          opacity: _fade,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 32),
            child: Column(
              children: [
                const Spacer(flex: 2),

                // Logo in card
                Container(
                  width: 84,
                  height: 84,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(22),
                    boxShadow: [
                      BoxShadow(
                        color: AppColors.primary.withOpacity(0.14),
                        blurRadius: 28,
                        offset: const Offset(0, 10),
                      ),
                      BoxShadow(
                        color: Colors.black.withOpacity(0.06),
                        blurRadius: 8,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: Center(
                    child: SvgPicture.asset(
                      'assets/images/physiolink_logo.svg',
                      width: 46,
                      height: 46,
                    ),
                  ),
                ),

                const SizedBox(height: 22),

                // ECG line
                AnimatedBuilder(
                  animation: _ecgProgress,
                  builder: (_, __) => SizedBox(
                    width: 130,
                    height: 36,
                    child: CustomPaint(
                      painter: _EcgLinePainter(
                        progress: _ecgProgress.value,
                        color: AppColors.primary,
                      ),
                    ),
                  ),
                ),

                const SizedBox(height: 18),

                // App name
                Text(
                  'PhysioLink',
                  style: AppTextStyles.largeTitle.copyWith(
                    color: AppColors.primary,
                    fontWeight: FontWeight.w800,
                    letterSpacing: -1.2,
                    fontSize: 36,
                  ),
                ),

                const SizedBox(height: 6),

                Text(
                  'Your recovery, guided.',
                  style: AppTextStyles.callout.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),

                const Spacer(flex: 3),

                // Three dots
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: List.generate(3, (i) {
                    return Container(
                      margin: const EdgeInsets.symmetric(horizontal: 3),
                      width: i == 0 ? 18 : 6,
                      height: 6,
                      decoration: BoxDecoration(
                        color: i == 0
                            ? AppColors.primary
                            : AppColors.primary.withOpacity(0.22),
                        borderRadius: BorderRadius.circular(3),
                      ),
                    );
                  }),
                ),

                const SizedBox(height: 14),

                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.lock_outline,
                      size: 10,
                      color: AppColors.textSecondary.withOpacity(0.45),
                    ),
                    const SizedBox(width: 4),
                    Text(
                      'HIPAA SECURED · V2.4.0',
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.textSecondary.withOpacity(0.45),
                        fontSize: 10,
                        letterSpacing: 0.5,
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 20),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

// ECG line painter — draws a flat line that transitions into a heartbeat spike
class _EcgLinePainter extends CustomPainter {
  const _EcgLinePainter({required this.progress, required this.color});

  final double progress;
  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    if (progress <= 0) return;

    final paint = Paint()
      ..color = color
      ..strokeWidth = 1.8
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round
      ..strokeJoin = StrokeJoin.round;

    final w = size.width;
    final h = size.height;
    final mid = h / 2;

    // Key ECG points
    final points = [
      Offset(0, mid),
      Offset(w * 0.28, mid),
      Offset(w * 0.34, mid + 5),
      Offset(w * 0.40, mid - h * 0.72),
      Offset(w * 0.46, mid + h * 0.32),
      Offset(w * 0.52, mid),
      Offset(w, mid),
    ];

    final totalLen = _segmentLengths(points).fold(0.0, (a, b) => a + b);
    final drawLen = totalLen * progress;

    final path = Path()..moveTo(points[0].dx, points[0].dy);
    double consumed = 0;
    final segLens = _segmentLengths(points);

    for (int i = 1; i < points.length; i++) {
      final segLen = segLens[i - 1];
      if (consumed + segLen <= drawLen) {
        path.lineTo(points[i].dx, points[i].dy);
        consumed += segLen;
      } else {
        final t = (drawLen - consumed) / segLen;
        final p = Offset.lerp(points[i - 1], points[i], t)!;
        path.lineTo(p.dx, p.dy);
        break;
      }
    }

    canvas.drawPath(path, paint);
  }

  List<double> _segmentLengths(List<Offset> pts) {
    return List.generate(
      pts.length - 1,
      (i) => (pts[i + 1] - pts[i]).distance,
    );
  }

  @override
  bool shouldRepaint(_EcgLinePainter old) => old.progress != progress;
}
