import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_ar.dart';
import 'app_localizations_en.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations? of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations);
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('ar'),
    Locale('en'),
  ];

  /// No description provided for @appTitle.
  ///
  /// In en, this message translates to:
  /// **'PhysioLink'**
  String get appTitle;

  /// No description provided for @home.
  ///
  /// In en, this message translates to:
  /// **'Home'**
  String get home;

  /// No description provided for @exercises.
  ///
  /// In en, this message translates to:
  /// **'Exercises'**
  String get exercises;

  /// No description provided for @appointments.
  ///
  /// In en, this message translates to:
  /// **'Appointments'**
  String get appointments;

  /// No description provided for @profile.
  ///
  /// In en, this message translates to:
  /// **'Profile'**
  String get profile;

  /// No description provided for @myExercises.
  ///
  /// In en, this message translates to:
  /// **'My Exercises'**
  String get myExercises;

  /// No description provided for @language.
  ///
  /// In en, this message translates to:
  /// **'Language'**
  String get language;

  /// No description provided for @english.
  ///
  /// In en, this message translates to:
  /// **'English'**
  String get english;

  /// No description provided for @arabic.
  ///
  /// In en, this message translates to:
  /// **'العربية'**
  String get arabic;

  /// No description provided for @retry.
  ///
  /// In en, this message translates to:
  /// **'Retry'**
  String get retry;

  /// No description provided for @cancel.
  ///
  /// In en, this message translates to:
  /// **'Cancel'**
  String get cancel;

  /// No description provided for @somethingWentWrong.
  ///
  /// In en, this message translates to:
  /// **'Something went wrong'**
  String get somethingWentWrong;

  /// No description provided for @unexpectedError.
  ///
  /// In en, this message translates to:
  /// **'An unexpected error occurred.'**
  String get unexpectedError;

  /// No description provided for @pullToRetry.
  ///
  /// In en, this message translates to:
  /// **'Pull to retry.'**
  String get pullToRetry;

  /// No description provided for @pullDownToRetry.
  ///
  /// In en, this message translates to:
  /// **'Pull down to retry.'**
  String get pullDownToRetry;

  /// No description provided for @noInternet.
  ///
  /// In en, this message translates to:
  /// **'No internet connection'**
  String get noInternet;

  /// No description provided for @serverError.
  ///
  /// In en, this message translates to:
  /// **'Server error. Try again.'**
  String get serverError;

  /// No description provided for @sessionExpired.
  ///
  /// In en, this message translates to:
  /// **'Session expired. Please log in again.'**
  String get sessionExpired;

  /// No description provided for @personalInfo.
  ///
  /// In en, this message translates to:
  /// **'Personal info'**
  String get personalInfo;

  /// No description provided for @preferences.
  ///
  /// In en, this message translates to:
  /// **'Preferences'**
  String get preferences;

  /// No description provided for @email.
  ///
  /// In en, this message translates to:
  /// **'EMAIL'**
  String get email;

  /// No description provided for @phone.
  ///
  /// In en, this message translates to:
  /// **'PHONE'**
  String get phone;

  /// No description provided for @clinic.
  ///
  /// In en, this message translates to:
  /// **'CLINIC'**
  String get clinic;

  /// No description provided for @therapist.
  ///
  /// In en, this message translates to:
  /// **'THERAPIST'**
  String get therapist;

  /// No description provided for @unassigned.
  ///
  /// In en, this message translates to:
  /// **'Unassigned'**
  String get unassigned;

  /// No description provided for @logOut.
  ///
  /// In en, this message translates to:
  /// **'Log out'**
  String get logOut;

  /// No description provided for @logOutQuestion.
  ///
  /// In en, this message translates to:
  /// **'Log Out?'**
  String get logOutQuestion;

  /// No description provided for @logOutBody.
  ///
  /// In en, this message translates to:
  /// **'You\'ll need to sign in again to access your account.'**
  String get logOutBody;

  /// No description provided for @couldntLoadProfile.
  ///
  /// In en, this message translates to:
  /// **'Couldn\'t load profile'**
  String get couldntLoadProfile;

  /// No description provided for @tagline.
  ///
  /// In en, this message translates to:
  /// **'Your partner in recovery wherever you are'**
  String get tagline;

  /// No description provided for @emailLabel.
  ///
  /// In en, this message translates to:
  /// **'Email'**
  String get emailLabel;

  /// No description provided for @password.
  ///
  /// In en, this message translates to:
  /// **'Password'**
  String get password;

  /// No description provided for @passwordHint.
  ///
  /// In en, this message translates to:
  /// **'Enter your password'**
  String get passwordHint;

  /// No description provided for @emailRequired.
  ///
  /// In en, this message translates to:
  /// **'Email is required'**
  String get emailRequired;

  /// No description provided for @passwordRequired.
  ///
  /// In en, this message translates to:
  /// **'Password is required'**
  String get passwordRequired;

  /// No description provided for @passwordMinLength.
  ///
  /// In en, this message translates to:
  /// **'Password must be at least 6 characters'**
  String get passwordMinLength;

  /// No description provided for @show.
  ///
  /// In en, this message translates to:
  /// **'Show'**
  String get show;

  /// No description provided for @hide.
  ///
  /// In en, this message translates to:
  /// **'Hide'**
  String get hide;

  /// No description provided for @logIn.
  ///
  /// In en, this message translates to:
  /// **'Log In'**
  String get logIn;

  /// No description provided for @incorrectCredentials.
  ///
  /// In en, this message translates to:
  /// **'Incorrect email or password.'**
  String get incorrectCredentials;

  /// No description provided for @securedByFooter.
  ///
  /// In en, this message translates to:
  /// **'Secured by PhysioLink · v2.4.0'**
  String get securedByFooter;

  /// No description provided for @hipaaSecured.
  ///
  /// In en, this message translates to:
  /// **'HIPAA SECURED · V2.4.0'**
  String get hipaaSecured;

  /// No description provided for @changePasswordTitle.
  ///
  /// In en, this message translates to:
  /// **'Set a new password'**
  String get changePasswordTitle;

  /// No description provided for @changePasswordSubtitle.
  ///
  /// In en, this message translates to:
  /// **'You\'re signed in with a temporary password. Choose a new one to continue.'**
  String get changePasswordSubtitle;

  /// No description provided for @currentPasswordLabel.
  ///
  /// In en, this message translates to:
  /// **'Temporary password'**
  String get currentPasswordLabel;

  /// No description provided for @currentPasswordHint.
  ///
  /// In en, this message translates to:
  /// **'Enter the temporary password'**
  String get currentPasswordHint;

  /// No description provided for @currentPasswordRequired.
  ///
  /// In en, this message translates to:
  /// **'Temporary password is required'**
  String get currentPasswordRequired;

  /// No description provided for @newPasswordLabel.
  ///
  /// In en, this message translates to:
  /// **'New password'**
  String get newPasswordLabel;

  /// No description provided for @newPasswordHint.
  ///
  /// In en, this message translates to:
  /// **'Enter a new password'**
  String get newPasswordHint;

  /// No description provided for @newPasswordRequired.
  ///
  /// In en, this message translates to:
  /// **'New password is required'**
  String get newPasswordRequired;

  /// No description provided for @confirmPasswordLabel.
  ///
  /// In en, this message translates to:
  /// **'Confirm password'**
  String get confirmPasswordLabel;

  /// No description provided for @confirmPasswordHint.
  ///
  /// In en, this message translates to:
  /// **'Re-enter the new password'**
  String get confirmPasswordHint;

  /// No description provided for @confirmPasswordRequired.
  ///
  /// In en, this message translates to:
  /// **'Please confirm your password'**
  String get confirmPasswordRequired;

  /// No description provided for @passwordsDoNotMatch.
  ///
  /// In en, this message translates to:
  /// **'Passwords don\'t match'**
  String get passwordsDoNotMatch;

  /// No description provided for @newPasswordSameAsTemporary.
  ///
  /// In en, this message translates to:
  /// **'New password must be different from the temporary one'**
  String get newPasswordSameAsTemporary;

  /// No description provided for @setPasswordCta.
  ///
  /// In en, this message translates to:
  /// **'Set password'**
  String get setPasswordCta;

  /// No description provided for @changePasswordError.
  ///
  /// In en, this message translates to:
  /// **'Couldn\'t update password. Check your temporary password and try again.'**
  String get changePasswordError;

  /// No description provided for @goodMorning.
  ///
  /// In en, this message translates to:
  /// **'Good morning'**
  String get goodMorning;

  /// No description provided for @goodAfternoon.
  ///
  /// In en, this message translates to:
  /// **'Good afternoon'**
  String get goodAfternoon;

  /// No description provided for @goodEvening.
  ///
  /// In en, this message translates to:
  /// **'Good evening'**
  String get goodEvening;

  /// No description provided for @greetingWithName.
  ///
  /// In en, this message translates to:
  /// **'{greeting}, {name}'**
  String greetingWithName(String greeting, String name);

  /// No description provided for @exercisesTodayStat.
  ///
  /// In en, this message translates to:
  /// **'Exercises · today'**
  String get exercisesTodayStat;

  /// No description provided for @streakDaysStat.
  ///
  /// In en, this message translates to:
  /// **'Streak · days'**
  String get streakDaysStat;

  /// No description provided for @nextTimeStat.
  ///
  /// In en, this message translates to:
  /// **'Next · {time}'**
  String nextTimeStat(String time);

  /// No description provided for @nextNoneStat.
  ///
  /// In en, this message translates to:
  /// **'Next · none'**
  String get nextNoneStat;

  /// No description provided for @todaysExercises.
  ///
  /// In en, this message translates to:
  /// **'Today\'s exercises'**
  String get todaysExercises;

  /// No description provided for @seeAll.
  ///
  /// In en, this message translates to:
  /// **'See all'**
  String get seeAll;

  /// No description provided for @couldntLoadExercises.
  ///
  /// In en, this message translates to:
  /// **'Couldn\'t load exercises'**
  String get couldntLoadExercises;

  /// No description provided for @noExercisesToday.
  ///
  /// In en, this message translates to:
  /// **'No exercises today'**
  String get noExercisesToday;

  /// No description provided for @noExercisesTodaySubtitle.
  ///
  /// In en, this message translates to:
  /// **'Your therapist hasn\'t assigned exercises yet.'**
  String get noExercisesTodaySubtitle;

  /// No description provided for @allDoneToday.
  ///
  /// In en, this message translates to:
  /// **'All done for today!'**
  String get allDoneToday;

  /// No description provided for @allDoneTodaySubtitle.
  ///
  /// In en, this message translates to:
  /// **'You\'ve completed all your exercises.'**
  String get allDoneTodaySubtitle;

  /// No description provided for @nextAppointmentTitle.
  ///
  /// In en, this message translates to:
  /// **'Next appointment'**
  String get nextAppointmentTitle;

  /// No description provided for @weeklyProgress.
  ///
  /// In en, this message translates to:
  /// **'Weekly progress'**
  String get weeklyProgress;

  /// No description provided for @sessionsProgress.
  ///
  /// In en, this message translates to:
  /// **'{completed} / {total} sessions'**
  String sessionsProgress(int completed, int total);

  /// No description provided for @noUpcomingAppointments.
  ///
  /// In en, this message translates to:
  /// **'No upcoming appointments'**
  String get noUpcomingAppointments;

  /// No description provided for @tapToRequestSession.
  ///
  /// In en, this message translates to:
  /// **'Tap to request a session with your therapist.'**
  String get tapToRequestSession;

  /// No description provided for @statusConfirmed.
  ///
  /// In en, this message translates to:
  /// **'Confirmed'**
  String get statusConfirmed;

  /// No description provided for @statusPending.
  ///
  /// In en, this message translates to:
  /// **'Pending'**
  String get statusPending;

  /// No description provided for @statusCancelled.
  ///
  /// In en, this message translates to:
  /// **'Cancelled'**
  String get statusCancelled;

  /// No description provided for @statusDone.
  ///
  /// In en, this message translates to:
  /// **'Done'**
  String get statusDone;

  /// No description provided for @searchExercises.
  ///
  /// In en, this message translates to:
  /// **'Search exercises'**
  String get searchExercises;

  /// No description provided for @filterAll.
  ///
  /// In en, this message translates to:
  /// **'All'**
  String get filterAll;

  /// No description provided for @filterActive.
  ///
  /// In en, this message translates to:
  /// **'Active'**
  String get filterActive;

  /// No description provided for @filterCompleted.
  ///
  /// In en, this message translates to:
  /// **'Completed'**
  String get filterCompleted;

  /// No description provided for @noMatchesFor.
  ///
  /// In en, this message translates to:
  /// **'No matches for \"{query}\"'**
  String noMatchesFor(String query);

  /// No description provided for @noCompletedExercises.
  ///
  /// In en, this message translates to:
  /// **'No completed exercises'**
  String get noCompletedExercises;

  /// No description provided for @noExercisesFound.
  ///
  /// In en, this message translates to:
  /// **'No exercises found'**
  String get noExercisesFound;

  /// No description provided for @searchNoMatchSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Try a different keyword, or browse your full exercise plan.'**
  String get searchNoMatchSubtitle;

  /// No description provided for @noExercisesAssignedSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Your therapist hasn\'t assigned any exercises yet.'**
  String get noExercisesAssignedSubtitle;

  /// No description provided for @clearSearch.
  ///
  /// In en, this message translates to:
  /// **'Clear search'**
  String get clearSearch;

  /// No description provided for @difficultyEasy.
  ///
  /// In en, this message translates to:
  /// **'Easy'**
  String get difficultyEasy;

  /// No description provided for @difficultyModerate.
  ///
  /// In en, this message translates to:
  /// **'Moderate'**
  String get difficultyModerate;

  /// No description provided for @difficultyHard.
  ///
  /// In en, this message translates to:
  /// **'Hard'**
  String get difficultyHard;

  /// No description provided for @setsCount.
  ///
  /// In en, this message translates to:
  /// **'{count} sets'**
  String setsCount(int count);

  /// No description provided for @repsCount.
  ///
  /// In en, this message translates to:
  /// **'{count} reps'**
  String repsCount(int count);

  /// No description provided for @minCount.
  ///
  /// In en, this message translates to:
  /// **'{count} min'**
  String minCount(int count);

  /// No description provided for @statSets.
  ///
  /// In en, this message translates to:
  /// **'SETS'**
  String get statSets;

  /// No description provided for @statReps.
  ///
  /// In en, this message translates to:
  /// **'REPS'**
  String get statReps;

  /// No description provided for @statDuration.
  ///
  /// In en, this message translates to:
  /// **'DURATION'**
  String get statDuration;

  /// No description provided for @instructions.
  ///
  /// In en, this message translates to:
  /// **'Instructions'**
  String get instructions;

  /// No description provided for @markComplete.
  ///
  /// In en, this message translates to:
  /// **'Mark complete'**
  String get markComplete;

  /// No description provided for @updateFeedback.
  ///
  /// In en, this message translates to:
  /// **'Update feedback'**
  String get updateFeedback;

  /// No description provided for @howDidThisFeel.
  ///
  /// In en, this message translates to:
  /// **'How did this feel?'**
  String get howDidThisFeel;

  /// No description provided for @feedbackHelpsTherapist.
  ///
  /// In en, this message translates to:
  /// **'Your feedback helps your therapist adjust your program.'**
  String get feedbackHelpsTherapist;

  /// No description provided for @rateEffortPrompt.
  ///
  /// In en, this message translates to:
  /// **'Rate the effort so your therapist can adjust your program.'**
  String get rateEffortPrompt;

  /// No description provided for @submitFeedback.
  ///
  /// In en, this message translates to:
  /// **'Submit Feedback'**
  String get submitFeedback;

  /// No description provided for @feedbackLogged.
  ///
  /// In en, this message translates to:
  /// **'Feedback logged'**
  String get feedbackLogged;

  /// No description provided for @feedbackLoggedOn.
  ///
  /// In en, this message translates to:
  /// **'Feedback logged {date}'**
  String feedbackLoggedOn(String date);

  /// No description provided for @edit.
  ///
  /// In en, this message translates to:
  /// **'Edit'**
  String get edit;

  /// No description provided for @veryEasyLabel.
  ///
  /// In en, this message translates to:
  /// **'Very easy'**
  String get veryEasyLabel;

  /// No description provided for @maxEffortLabel.
  ///
  /// In en, this message translates to:
  /// **'Max effort'**
  String get maxEffortLabel;

  /// No description provided for @ratingOutOfTen.
  ///
  /// In en, this message translates to:
  /// **'{rating} / 10'**
  String ratingOutOfTen(int rating);

  /// No description provided for @videoPreviewUnavailable.
  ///
  /// In en, this message translates to:
  /// **'Video preview unavailable'**
  String get videoPreviewUnavailable;

  /// No description provided for @rateHowFeltA11y.
  ///
  /// In en, this message translates to:
  /// **'Rate how this exercise felt'**
  String get rateHowFeltA11y;

  /// No description provided for @effortRatedA11y.
  ///
  /// In en, this message translates to:
  /// **'Effort rated {label}, {rating} out of 10. Tap to edit.'**
  String effortRatedA11y(String label, int rating);

  /// No description provided for @feedbackLoggedToday.
  ///
  /// In en, this message translates to:
  /// **'today'**
  String get feedbackLoggedToday;

  /// No description provided for @feedbackLoggedYesterday.
  ///
  /// In en, this message translates to:
  /// **'yesterday'**
  String get feedbackLoggedYesterday;

  /// No description provided for @howIntenseWasIt.
  ///
  /// In en, this message translates to:
  /// **'How intense was it?'**
  String get howIntenseWasIt;

  /// No description provided for @legendEasy.
  ///
  /// In en, this message translates to:
  /// **'Easy'**
  String get legendEasy;

  /// No description provided for @legendModerate.
  ///
  /// In en, this message translates to:
  /// **'Moderate'**
  String get legendModerate;

  /// No description provided for @legendMaximum.
  ///
  /// In en, this message translates to:
  /// **'Maximum'**
  String get legendMaximum;

  /// No description provided for @selectedEffort.
  ///
  /// In en, this message translates to:
  /// **'{value} — {label}'**
  String selectedEffort(int value, String label);

  /// No description provided for @effort1.
  ///
  /// In en, this message translates to:
  /// **'Very Easy'**
  String get effort1;

  /// No description provided for @effort2.
  ///
  /// In en, this message translates to:
  /// **'Easy'**
  String get effort2;

  /// No description provided for @effort3.
  ///
  /// In en, this message translates to:
  /// **'Fairly Easy'**
  String get effort3;

  /// No description provided for @effort4.
  ///
  /// In en, this message translates to:
  /// **'Somewhat Easy'**
  String get effort4;

  /// No description provided for @effort5.
  ///
  /// In en, this message translates to:
  /// **'Moderate'**
  String get effort5;

  /// No description provided for @effort6.
  ///
  /// In en, this message translates to:
  /// **'Somewhat Hard'**
  String get effort6;

  /// No description provided for @effort7.
  ///
  /// In en, this message translates to:
  /// **'Fairly Hard'**
  String get effort7;

  /// No description provided for @effort8.
  ///
  /// In en, this message translates to:
  /// **'Hard'**
  String get effort8;

  /// No description provided for @effort9.
  ///
  /// In en, this message translates to:
  /// **'Very Hard'**
  String get effort9;

  /// No description provided for @effort10.
  ///
  /// In en, this message translates to:
  /// **'Maximum Effort'**
  String get effort10;

  /// No description provided for @effortLevel.
  ///
  /// In en, this message translates to:
  /// **'Level {value}'**
  String effortLevel(int value);

  /// No description provided for @couldntLoadAppointments.
  ///
  /// In en, this message translates to:
  /// **'Couldn\'t load appointments'**
  String get couldntLoadAppointments;

  /// No description provided for @nothingOnBooks.
  ///
  /// In en, this message translates to:
  /// **'Nothing on the books'**
  String get nothingOnBooks;

  /// No description provided for @nothingOnBooksSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Ready for your next session?\nRequest a time and your clinic will confirm.'**
  String get nothingOnBooksSubtitle;

  /// No description provided for @requestAppointmentCta.
  ///
  /// In en, this message translates to:
  /// **'+ Request appointment'**
  String get requestAppointmentCta;

  /// No description provided for @request.
  ///
  /// In en, this message translates to:
  /// **'Request'**
  String get request;

  /// No description provided for @upcomingCount.
  ///
  /// In en, this message translates to:
  /// **'Upcoming · {count}'**
  String upcomingCount(int count);

  /// No description provided for @pastCount.
  ///
  /// In en, this message translates to:
  /// **'Past · {count}'**
  String pastCount(int count);

  /// No description provided for @requestAppointment.
  ///
  /// In en, this message translates to:
  /// **'Request Appointment'**
  String get requestAppointment;

  /// No description provided for @dateLabel.
  ///
  /// In en, this message translates to:
  /// **'Date'**
  String get dateLabel;

  /// No description provided for @timeLabel.
  ///
  /// In en, this message translates to:
  /// **'Time'**
  String get timeLabel;

  /// No description provided for @notesOptional.
  ///
  /// In en, this message translates to:
  /// **'Notes (optional)'**
  String get notesOptional;

  /// No description provided for @selectDate.
  ///
  /// In en, this message translates to:
  /// **'Select a date'**
  String get selectDate;

  /// No description provided for @selectTime.
  ///
  /// In en, this message translates to:
  /// **'Select a time'**
  String get selectTime;

  /// No description provided for @pleaseSelectDate.
  ///
  /// In en, this message translates to:
  /// **'Please select a date'**
  String get pleaseSelectDate;

  /// No description provided for @pleaseSelectTime.
  ///
  /// In en, this message translates to:
  /// **'Please select a time'**
  String get pleaseSelectTime;

  /// No description provided for @notesHint.
  ///
  /// In en, this message translates to:
  /// **'Describe what you\'d like to discuss or any concerns…'**
  String get notesHint;

  /// No description provided for @appointmentRequested.
  ///
  /// In en, this message translates to:
  /// **'Appointment requested successfully'**
  String get appointmentRequested;

  /// No description provided for @therapistWillConfirm.
  ///
  /// In en, this message translates to:
  /// **'Your therapist will confirm or suggest an alternative time.'**
  String get therapistWillConfirm;

  /// No description provided for @noInternetPeriod.
  ///
  /// In en, this message translates to:
  /// **'No internet connection.'**
  String get noInternetPeriod;

  /// No description provided for @serverErrorWithCode.
  ///
  /// In en, this message translates to:
  /// **'Server error {code}. Check logs.'**
  String serverErrorWithCode(String code);
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['ar', 'en'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'ar':
      return AppLocalizationsAr();
    case 'en':
      return AppLocalizationsEn();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
