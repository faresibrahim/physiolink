// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Arabic (`ar`).
class AppLocalizationsAr extends AppLocalizations {
  AppLocalizationsAr([String locale = 'ar']) : super(locale);

  @override
  String get appTitle => 'فيزيولينك';

  @override
  String get home => 'الرئيسية';

  @override
  String get exercises => 'التمارين';

  @override
  String get appointments => 'المواعيد';

  @override
  String get profile => 'الملف الشخصي';

  @override
  String get myExercises => 'تماريني';

  @override
  String get language => 'اللغة';

  @override
  String get english => 'English';

  @override
  String get arabic => 'العربية';

  @override
  String get retry => 'إعادة المحاولة';

  @override
  String get cancel => 'إلغاء';

  @override
  String get somethingWentWrong => 'حدث خطأ ما';

  @override
  String get unexpectedError => 'حدث خطأ غير متوقع.';

  @override
  String get pullToRetry => 'اسحب لإعادة المحاولة.';

  @override
  String get pullDownToRetry => 'اسحب للأسفل لإعادة المحاولة.';

  @override
  String get noInternet => 'لا يوجد اتصال بالإنترنت';

  @override
  String get serverError => 'خطأ في الخادم. حاول مرة أخرى.';

  @override
  String get sessionExpired => 'انتهت الجلسة. يرجى تسجيل الدخول مرة أخرى.';

  @override
  String get personalInfo => 'المعلومات الشخصية';

  @override
  String get preferences => 'التفضيلات';

  @override
  String get email => 'البريد الإلكتروني';

  @override
  String get phone => 'رقم الهاتف';

  @override
  String get clinic => 'العيادة';

  @override
  String get therapist => 'المعالج';

  @override
  String get unassigned => 'غير محدد';

  @override
  String get logOut => 'تسجيل الخروج';

  @override
  String get logOutQuestion => 'تسجيل الخروج؟';

  @override
  String get logOutBody => 'ستحتاج إلى تسجيل الدخول مرة أخرى للوصول إلى حسابك.';

  @override
  String get couldntLoadProfile => 'تعذّر تحميل الملف الشخصي';

  @override
  String get tagline => 'شريكك في التعافي أينما كنت';

  @override
  String get emailLabel => 'البريد الإلكتروني';

  @override
  String get password => 'كلمة المرور';

  @override
  String get passwordHint => 'أدخل كلمة المرور';

  @override
  String get emailRequired => 'البريد الإلكتروني مطلوب';

  @override
  String get passwordRequired => 'كلمة المرور مطلوبة';

  @override
  String get passwordMinLength =>
      'يجب أن تتكون كلمة المرور من 6 أحرف على الأقل';

  @override
  String get show => 'إظهار';

  @override
  String get hide => 'إخفاء';

  @override
  String get logIn => 'تسجيل الدخول';

  @override
  String get incorrectCredentials =>
      'البريد الإلكتروني أو كلمة المرور غير صحيحة.';

  @override
  String get securedByFooter => 'محمي بواسطة PhysioLink · v2.4.0';

  @override
  String get hipaaSecured => 'محمي وفق HIPAA · V2.4.0';

  @override
  String get changePasswordTitle => 'تعيين كلمة مرور جديدة';

  @override
  String get changePasswordSubtitle =>
      'لقد سجّلت الدخول بكلمة مرور مؤقتة. اختر كلمة مرور جديدة للمتابعة.';

  @override
  String get currentPasswordLabel => 'كلمة المرور المؤقتة';

  @override
  String get currentPasswordHint => 'أدخل كلمة المرور المؤقتة';

  @override
  String get currentPasswordRequired => 'كلمة المرور المؤقتة مطلوبة';

  @override
  String get newPasswordLabel => 'كلمة المرور الجديدة';

  @override
  String get newPasswordHint => 'أدخل كلمة مرور جديدة';

  @override
  String get newPasswordRequired => 'كلمة المرور الجديدة مطلوبة';

  @override
  String get confirmPasswordLabel => 'تأكيد كلمة المرور';

  @override
  String get confirmPasswordHint => 'أعد إدخال كلمة المرور الجديدة';

  @override
  String get confirmPasswordRequired => 'يرجى تأكيد كلمة المرور';

  @override
  String get passwordsDoNotMatch => 'كلمتا المرور غير متطابقتين';

  @override
  String get newPasswordSameAsTemporary =>
      'يجب أن تختلف كلمة المرور الجديدة عن المؤقتة';

  @override
  String get setPasswordCta => 'تعيين كلمة المرور';

  @override
  String get changePasswordError =>
      'تعذّر تحديث كلمة المرور. تحقق من كلمة المرور المؤقتة وحاول مرة أخرى.';

  @override
  String get goodMorning => 'صباح الخير';

  @override
  String get goodAfternoon => 'مساء الخير';

  @override
  String get goodEvening => 'مساء الخير';

  @override
  String greetingWithName(String greeting, String name) {
    return '$greeting، $name';
  }

  @override
  String get exercisesTodayStat => 'التمارين · اليوم';

  @override
  String get streakDaysStat => 'التتابع · أيام';

  @override
  String nextTimeStat(String time) {
    return 'التالي · $time';
  }

  @override
  String get nextNoneStat => 'التالي · لا يوجد';

  @override
  String get todaysExercises => 'تمارين اليوم';

  @override
  String get seeAll => 'عرض الكل';

  @override
  String get couldntLoadExercises => 'تعذّر تحميل التمارين';

  @override
  String get noExercisesToday => 'لا توجد تمارين اليوم';

  @override
  String get noExercisesTodaySubtitle => 'لم يقم معالجك بتعيين تمارين بعد.';

  @override
  String get allDoneToday => 'انتهيت من كل شيء لهذا اليوم!';

  @override
  String get allDoneTodaySubtitle => 'لقد أكملت جميع تمارينك.';

  @override
  String get nextAppointmentTitle => 'الموعد التالي';

  @override
  String get weeklyProgress => 'التقدم الأسبوعي';

  @override
  String sessionsProgress(int completed, int total) {
    return '$completed / $total جلسة';
  }

  @override
  String get noUpcomingAppointments => 'لا توجد مواعيد قادمة';

  @override
  String get tapToRequestSession => 'اضغط لطلب جلسة مع معالجك.';

  @override
  String get statusConfirmed => 'مؤكد';

  @override
  String get statusPending => 'قيد الانتظار';

  @override
  String get statusCancelled => 'ملغى';

  @override
  String get statusDone => 'منتهٍ';

  @override
  String get searchExercises => 'ابحث عن التمارين';

  @override
  String get filterAll => 'الكل';

  @override
  String get filterActive => 'نشط';

  @override
  String get filterCompleted => 'مكتمل';

  @override
  String noMatchesFor(String query) {
    return 'لا نتائج لـ \"$query\"';
  }

  @override
  String get noCompletedExercises => 'لا توجد تمارين مكتملة';

  @override
  String get noExercisesFound => 'لم يتم العثور على تمارين';

  @override
  String get searchNoMatchSubtitle =>
      'جرّب كلمة مختلفة، أو تصفّح خطة تمارينك الكاملة.';

  @override
  String get noExercisesAssignedSubtitle =>
      'لم يقم معالجك بتعيين أي تمارين بعد.';

  @override
  String get clearSearch => 'مسح البحث';

  @override
  String get difficultyEasy => 'سهل';

  @override
  String get difficultyModerate => 'متوسط';

  @override
  String get difficultyHard => 'صعب';

  @override
  String setsCount(int count) {
    return '$count مجموعات';
  }

  @override
  String repsCount(int count) {
    return '$count تكرار';
  }

  @override
  String minCount(int count) {
    return '$count دقيقة';
  }

  @override
  String get statSets => 'المجموعات';

  @override
  String get statReps => 'التكرارات';

  @override
  String get statDuration => 'المدة';

  @override
  String get instructions => 'التعليمات';

  @override
  String get markComplete => 'تحديد كمكتمل';

  @override
  String get updateFeedback => 'تحديث التقييم';

  @override
  String get howDidThisFeel => 'كيف كان شعورك؟';

  @override
  String get feedbackHelpsTherapist => 'يساعد تقييمك معالجك على تعديل برنامجك.';

  @override
  String get rateEffortPrompt =>
      'قيّم الجهد حتى يتمكن معالجك من تعديل برنامجك.';

  @override
  String get submitFeedback => 'إرسال التقييم';

  @override
  String get feedbackLogged => 'تم تسجيل التقييم';

  @override
  String feedbackLoggedOn(String date) {
    return 'تم تسجيل التقييم $date';
  }

  @override
  String get edit => 'تعديل';

  @override
  String get veryEasyLabel => 'سهل جداً';

  @override
  String get maxEffortLabel => 'أقصى جهد';

  @override
  String ratingOutOfTen(int rating) {
    return '$rating / 10';
  }

  @override
  String get videoPreviewUnavailable => 'معاينة الفيديو غير متاحة';

  @override
  String get rateHowFeltA11y => 'قيّم شعورك بهذا التمرين';

  @override
  String effortRatedA11y(String label, int rating) {
    return 'تم تقييم الجهد بـ $label، $rating من 10. اضغط للتعديل.';
  }

  @override
  String get feedbackLoggedToday => 'اليوم';

  @override
  String get feedbackLoggedYesterday => 'أمس';

  @override
  String get howIntenseWasIt => 'ما مدى شدّته؟';

  @override
  String get legendEasy => 'سهل';

  @override
  String get legendModerate => 'متوسط';

  @override
  String get legendMaximum => 'أقصى';

  @override
  String selectedEffort(int value, String label) {
    return '$value — $label';
  }

  @override
  String get effort1 => 'سهل جداً';

  @override
  String get effort2 => 'سهل';

  @override
  String get effort3 => 'سهل نوعاً ما';

  @override
  String get effort4 => 'سهل إلى حد ما';

  @override
  String get effort5 => 'متوسط';

  @override
  String get effort6 => 'صعب إلى حد ما';

  @override
  String get effort7 => 'صعب نوعاً ما';

  @override
  String get effort8 => 'صعب';

  @override
  String get effort9 => 'صعب جداً';

  @override
  String get effort10 => 'أقصى جهد';

  @override
  String effortLevel(int value) {
    return 'المستوى $value';
  }

  @override
  String get couldntLoadAppointments => 'تعذّر تحميل المواعيد';

  @override
  String get nothingOnBooks => 'لا توجد مواعيد';

  @override
  String get nothingOnBooksSubtitle =>
      'هل أنت مستعد لجلستك القادمة؟\nاطلب موعداً وستؤكده عيادتك.';

  @override
  String get requestAppointmentCta => '+ طلب موعد';

  @override
  String get request => 'طلب';

  @override
  String upcomingCount(int count) {
    return 'القادمة · $count';
  }

  @override
  String pastCount(int count) {
    return 'السابقة · $count';
  }

  @override
  String get requestAppointment => 'طلب موعد';

  @override
  String get dateLabel => 'التاريخ';

  @override
  String get timeLabel => 'الوقت';

  @override
  String get notesOptional => 'ملاحظات (اختياري)';

  @override
  String get selectDate => 'اختر تاريخاً';

  @override
  String get selectTime => 'اختر وقتاً';

  @override
  String get pleaseSelectDate => 'يرجى اختيار تاريخ';

  @override
  String get pleaseSelectTime => 'يرجى اختيار وقت';

  @override
  String get notesHint => 'صف ما تودّ مناقشته أو أي مخاوف لديك…';

  @override
  String get appointmentRequested => 'تم طلب الموعد بنجاح';

  @override
  String get therapistWillConfirm =>
      'سيؤكد معالجك الموعد أو يقترح وقتاً بديلاً.';

  @override
  String get noInternetPeriod => 'لا يوجد اتصال بالإنترنت.';

  @override
  String serverErrorWithCode(String code) {
    return 'خطأ في الخادم $code. تحقق من السجلات.';
  }
}
