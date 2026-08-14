// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'PhysioLink';

  @override
  String get home => 'Home';

  @override
  String get exercises => 'Exercises';

  @override
  String get appointments => 'Appointments';

  @override
  String get profile => 'Profile';

  @override
  String get myExercises => 'My Exercises';

  @override
  String get language => 'Language';

  @override
  String get english => 'English';

  @override
  String get arabic => 'العربية';

  @override
  String get retry => 'Retry';

  @override
  String get cancel => 'Cancel';

  @override
  String get somethingWentWrong => 'Something went wrong';

  @override
  String get unexpectedError => 'An unexpected error occurred.';

  @override
  String get pullToRetry => 'Pull to retry.';

  @override
  String get pullDownToRetry => 'Pull down to retry.';

  @override
  String get noInternet => 'No internet connection';

  @override
  String get serverError => 'Server error. Try again.';

  @override
  String get sessionExpired => 'Session expired. Please log in again.';

  @override
  String get personalInfo => 'Personal info';

  @override
  String get preferences => 'Preferences';

  @override
  String get email => 'EMAIL';

  @override
  String get phone => 'PHONE';

  @override
  String get clinic => 'CLINIC';

  @override
  String get therapist => 'THERAPIST';

  @override
  String get unassigned => 'Unassigned';

  @override
  String get logOut => 'Log out';

  @override
  String get logOutQuestion => 'Log Out?';

  @override
  String get logOutBody =>
      'You\'ll need to sign in again to access your account.';

  @override
  String get couldntLoadProfile => 'Couldn\'t load profile';

  @override
  String get tagline => 'Your partner in recovery wherever you are';

  @override
  String get emailLabel => 'Email';

  @override
  String get password => 'Password';

  @override
  String get passwordHint => 'Enter your password';

  @override
  String get emailRequired => 'Email is required';

  @override
  String get passwordRequired => 'Password is required';

  @override
  String get passwordMinLength => 'Password must be at least 6 characters';

  @override
  String get show => 'Show';

  @override
  String get hide => 'Hide';

  @override
  String get logIn => 'Log In';

  @override
  String get incorrectCredentials => 'Incorrect email or password.';

  @override
  String get securedByFooter => 'Secured by PhysioLink · v2.4.0';

  @override
  String get hipaaSecured => 'HIPAA SECURED · V2.4.0';

  @override
  String get changePasswordTitle => 'Set a new password';

  @override
  String get changePasswordSubtitle =>
      'You\'re signed in with a temporary password. Choose a new one to continue.';

  @override
  String get currentPasswordLabel => 'Temporary password';

  @override
  String get currentPasswordHint => 'Enter the temporary password';

  @override
  String get currentPasswordRequired => 'Temporary password is required';

  @override
  String get newPasswordLabel => 'New password';

  @override
  String get newPasswordHint => 'Enter a new password';

  @override
  String get newPasswordRequired => 'New password is required';

  @override
  String get confirmPasswordLabel => 'Confirm password';

  @override
  String get confirmPasswordHint => 'Re-enter the new password';

  @override
  String get confirmPasswordRequired => 'Please confirm your password';

  @override
  String get passwordsDoNotMatch => 'Passwords don\'t match';

  @override
  String get newPasswordSameAsTemporary =>
      'New password must be different from the temporary one';

  @override
  String get setPasswordCta => 'Set password';

  @override
  String get changePasswordError =>
      'Couldn\'t update password. Check your temporary password and try again.';

  @override
  String get goodMorning => 'Good morning';

  @override
  String get goodAfternoon => 'Good afternoon';

  @override
  String get goodEvening => 'Good evening';

  @override
  String greetingWithName(String greeting, String name) {
    return '$greeting, $name';
  }

  @override
  String get exercisesTodayStat => 'Exercises · today';

  @override
  String get streakDaysStat => 'Streak · days';

  @override
  String nextTimeStat(String time) {
    return 'Next · $time';
  }

  @override
  String get nextNoneStat => 'Next · none';

  @override
  String get todaysExercises => 'Today\'s exercises';

  @override
  String get seeAll => 'See all';

  @override
  String get couldntLoadExercises => 'Couldn\'t load exercises';

  @override
  String get noExercisesToday => 'No exercises today';

  @override
  String get noExercisesTodaySubtitle =>
      'Your therapist hasn\'t assigned exercises yet.';

  @override
  String get allDoneToday => 'All done for today!';

  @override
  String get allDoneTodaySubtitle => 'You\'ve completed all your exercises.';

  @override
  String get nextAppointmentTitle => 'Next appointment';

  @override
  String get weeklyProgress => 'Weekly progress';

  @override
  String sessionsProgress(int completed, int total) {
    return '$completed / $total sessions';
  }

  @override
  String get noUpcomingAppointments => 'No upcoming appointments';

  @override
  String get tapToRequestSession =>
      'Tap to request a session with your therapist.';

  @override
  String get statusConfirmed => 'Confirmed';

  @override
  String get statusPending => 'Pending';

  @override
  String get statusCancelled => 'Cancelled';

  @override
  String get statusDone => 'Done';

  @override
  String get searchExercises => 'Search exercises';

  @override
  String get filterAll => 'All';

  @override
  String get filterActive => 'Active';

  @override
  String get filterCompleted => 'Completed';

  @override
  String noMatchesFor(String query) {
    return 'No matches for \"$query\"';
  }

  @override
  String get noCompletedExercises => 'No completed exercises';

  @override
  String get noExercisesFound => 'No exercises found';

  @override
  String get searchNoMatchSubtitle =>
      'Try a different keyword, or browse your full exercise plan.';

  @override
  String get noExercisesAssignedSubtitle =>
      'Your therapist hasn\'t assigned any exercises yet.';

  @override
  String get clearSearch => 'Clear search';

  @override
  String get difficultyEasy => 'Easy';

  @override
  String get difficultyModerate => 'Moderate';

  @override
  String get difficultyHard => 'Hard';

  @override
  String setsCount(int count) {
    return '$count sets';
  }

  @override
  String repsCount(int count) {
    return '$count reps';
  }

  @override
  String minCount(int count) {
    return '$count min';
  }

  @override
  String get statSets => 'SETS';

  @override
  String get statReps => 'REPS';

  @override
  String get statDuration => 'DURATION';

  @override
  String get instructions => 'Instructions';

  @override
  String get markComplete => 'Mark complete';

  @override
  String get updateFeedback => 'Update feedback';

  @override
  String get howDidThisFeel => 'How did this feel?';

  @override
  String get feedbackHelpsTherapist =>
      'Your feedback helps your therapist adjust your program.';

  @override
  String get rateEffortPrompt =>
      'Rate the effort so your therapist can adjust your program.';

  @override
  String get submitFeedback => 'Submit Feedback';

  @override
  String get feedbackLogged => 'Feedback logged';

  @override
  String feedbackLoggedOn(String date) {
    return 'Feedback logged $date';
  }

  @override
  String get edit => 'Edit';

  @override
  String get veryEasyLabel => 'Very easy';

  @override
  String get maxEffortLabel => 'Max effort';

  @override
  String ratingOutOfTen(int rating) {
    return '$rating / 10';
  }

  @override
  String get videoPreviewUnavailable => 'Video preview unavailable';

  @override
  String get rateHowFeltA11y => 'Rate how this exercise felt';

  @override
  String effortRatedA11y(String label, int rating) {
    return 'Effort rated $label, $rating out of 10. Tap to edit.';
  }

  @override
  String get feedbackLoggedToday => 'today';

  @override
  String get feedbackLoggedYesterday => 'yesterday';

  @override
  String get howIntenseWasIt => 'How intense was it?';

  @override
  String get legendEasy => 'Easy';

  @override
  String get legendModerate => 'Moderate';

  @override
  String get legendMaximum => 'Maximum';

  @override
  String selectedEffort(int value, String label) {
    return '$value — $label';
  }

  @override
  String get effort1 => 'Very Easy';

  @override
  String get effort2 => 'Easy';

  @override
  String get effort3 => 'Fairly Easy';

  @override
  String get effort4 => 'Somewhat Easy';

  @override
  String get effort5 => 'Moderate';

  @override
  String get effort6 => 'Somewhat Hard';

  @override
  String get effort7 => 'Fairly Hard';

  @override
  String get effort8 => 'Hard';

  @override
  String get effort9 => 'Very Hard';

  @override
  String get effort10 => 'Maximum Effort';

  @override
  String effortLevel(int value) {
    return 'Level $value';
  }

  @override
  String get couldntLoadAppointments => 'Couldn\'t load appointments';

  @override
  String get nothingOnBooks => 'Nothing on the books';

  @override
  String get nothingOnBooksSubtitle =>
      'Ready for your next session?\nRequest a time and your clinic will confirm.';

  @override
  String get requestAppointmentCta => '+ Request appointment';

  @override
  String get request => 'Request';

  @override
  String upcomingCount(int count) {
    return 'Upcoming · $count';
  }

  @override
  String pastCount(int count) {
    return 'Past · $count';
  }

  @override
  String get requestAppointment => 'Request Appointment';

  @override
  String get dateLabel => 'Date';

  @override
  String get timeLabel => 'Time';

  @override
  String get notesOptional => 'Notes (optional)';

  @override
  String get selectDate => 'Select a date';

  @override
  String get selectTime => 'Select a time';

  @override
  String get pleaseSelectDate => 'Please select a date';

  @override
  String get pleaseSelectTime => 'Please select a time';

  @override
  String get notesHint =>
      'Describe what you\'d like to discuss or any concerns…';

  @override
  String get appointmentRequested => 'Appointment requested successfully';

  @override
  String get therapistWillConfirm =>
      'Your therapist will confirm or suggest an alternative time.';

  @override
  String get noInternetPeriod => 'No internet connection.';

  @override
  String serverErrorWithCode(String code) {
    return 'Server error $code. Check logs.';
  }

  @override
  String get statusRequested => 'Pending — awaiting confirmation';

  @override
  String get statusRejected => 'Rejected';

  @override
  String get statusExpired => 'Expired';

  @override
  String get statusCancelledByClinic => 'Cancelled by clinic';

  @override
  String get bookAppointment => 'Book Appointment';

  @override
  String get chooseSlotSubtitle => 'Choose an open time with your therapist';

  @override
  String get noSlotsTitle => 'No open slots';

  @override
  String get noSlotsSubtitle =>
      'There are no available times right now. If you haven\'t been assigned a therapist yet, please contact your clinic.';

  @override
  String get slotTaken => 'That slot was just taken. Please pick another.';

  @override
  String get requestSlotCta => 'Request this slot';

  @override
  String get requestSentPending => 'Request sent — awaiting confirmation';

  @override
  String get slotDurationHint => '45 min';

  @override
  String get slotBooked => 'Booked';

  @override
  String get chooseTimeTitle => 'Pick a time';

  @override
  String get confirmRequestTitle => 'Confirm your request';
}
