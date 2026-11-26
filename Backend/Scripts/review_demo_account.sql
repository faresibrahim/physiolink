-- =============================================================================
-- App Store / Google Play review demo account
-- =============================================================================
-- Creates (or resets) a fully isolated demo setup for store reviewers:
--   * its own clinic "PhysioLink Review Demo" — clinic isolation means nobody in
--     it can see any real clinic's patients, and no real clinic can see it;
--   * one fictional therapist, one fictional patient (username: applereview);
--   * 4 active exercise assignments, one confirmed upcoming appointment, one past
--     completed appointment, and 3 open booking slots for the coming days.
-- All names, phone numbers and the diagnosis are fictional.
--
-- The account signs in directly: MustChangePassword = false.
-- The password is NOT in this file. Generate its hash with hash_password.py and
-- pass it in:
--
--   python Backend/Scripts/hash_password.py
--   psql "<connection string>" -v pw_hash='<hash>' -f Backend/Scripts/review_demo_account.sql
--
-- Safe to re-run: it resets the password and un-deletes the account, keeps the
-- existing exercises, and tops up the open slots / upcoming appointment if they
-- have passed.
-- =============================================================================

\set ON_ERROR_STOP on

\if :{?pw_hash}
\else
  \echo 'Missing -v pw_hash=... (generate it with hash_password.py)'
  \quit
\endif
-- Reject an empty or malformed hash (an Identity v3 hash is base64 starting "AQAAAA").
SELECT (:'pw_hash' ~ '^AQAAAA[A-Za-z0-9+/=]{60,}$') AS pw_hash_ok \gset
\if :pw_hash_ok
\else
  \echo 'pw_hash is empty or not an ASP.NET Identity v3 hash — nothing changed.'
  \quit
\endif

BEGIN;

-- Fixed ids so the script is idempotent and the demo rows are easy to find.
\set clinic_id    '''d0000000-0000-4000-8000-00000000c001'''
\set therapist_id '''d0000000-0000-4000-8000-00000000a001'''
\set user_id      '''d0000000-0000-4000-8000-00000000b001'''
\set patient_id   '''d0000000-0000-4000-8000-00000000e001'''

INSERT INTO "Clinics" ("ClinicId", "Name", "Address", "Email", "PhoneNumber",
                       "OpenHour", "CloseHour", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
VALUES (:clinic_id, 'PhysioLink Review Demo', 'Demo address (not a real clinic)',
        'demo-clinic@example.com', '+000000000000', 8, 18, true, false, now(), now())
ON CONFLICT ("ClinicId") DO UPDATE SET "IsActive" = true, "IsDeleted" = false, "UpdatedAt" = now();

INSERT INTO "Therapists" ("TherapistId", "ClinicId", "FirstName", "LastName", "Email",
                          "PhoneNumber", "Speciality", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
VALUES (:therapist_id, :clinic_id, 'Demo', 'Therapist', 'demo-therapist@example.com',
        '+000000000001', 'Physiotherapy', true, false, now(), now())
ON CONFLICT ("TherapistId") DO UPDATE SET "IsActive" = true, "IsDeleted" = false, "UpdatedAt" = now();

INSERT INTO "Users" ("ApplicationUserId", "ClinicId", "FirstName", "LastName", "Email", "Username",
                     "PasswordHash", "Role", "MustChangePassword", "IsDeleted", "CreatedAt", "UpdatedAt")
VALUES (:user_id, :clinic_id, 'Apple', 'Review', NULL, 'applereview',
        :'pw_hash', 'Patient', false, false, now(), now())
ON CONFLICT ("ApplicationUserId") DO UPDATE
  SET "PasswordHash" = EXCLUDED."PasswordHash",
      "MustChangePassword" = false,
      "IsDeleted" = false,
      "RefreshToken" = NULL,
      "RefreshTokenExpiry" = NULL,
      "UpdatedAt" = now();

INSERT INTO "Patients" ("PatientId", "ApplicationUserId", "ClinicId", "TherapistId", "FirstName", "LastName",
                        "Username", "Email", "PhoneNumber", "Diagnosis", "IsActive", "IsDeleted",
                        "ReminderEnabled", "ReminderTime", "CreatedAt", "UpdatedAt")
VALUES (:patient_id, :user_id, :clinic_id, :therapist_id, 'Apple', 'Review',
        'applereview', NULL, '+000000000002', 'Demo account: lower back stiffness (sample data)',
        true, false, false, '18:00', now(), now())
ON CONFLICT ("PatientId") DO UPDATE SET "IsActive" = true, "IsDeleted" = false, "UpdatedAt" = now();

-- Exercise program: 4 exercises from the shared catalog (only if none assigned yet).
INSERT INTO "ExerciseAssignments" ("ExerciseAssignmentId", "PatientId", "ExerciseId", "TherapistName",
                                   "Sets", "Reps", "DurationMinutes", "FrequencyPerWeek", "FrequencyPerDay",
                                   "Status", "AssignedAt", "IsDeleted", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), :patient_id, e."ExerciseId", 'Demo Therapist',
       e."Sets", e."Reps", e."DurationMinutes", 3, 1,
       0 /* Active */, now(), false, now(), now()
FROM (
  SELECT * FROM "Exercises" WHERE NOT "IsDeleted" ORDER BY "Name" LIMIT 4
) e
WHERE NOT EXISTS (
  SELECT 1 FROM "ExerciseAssignments" WHERE "PatientId" = :patient_id AND NOT "IsDeleted"
);

-- A past, completed appointment (only once).
INSERT INTO "Appointments" ("AppointmentId", "PatientId", "TherapistId", "ClinicId", "TherapistName",
                            "Title", "AppointmentTime", "Status", "Notes", "IsDeleted", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), :patient_id, :therapist_id, :clinic_id, 'Demo Therapist',
       'Initial assessment', date_trunc('day', now()) - interval '7 days' + interval '8 hours',
       2 /* Completed */, 'Sample appointment for review.', false, now(), now()
WHERE NOT EXISTS (
  SELECT 1 FROM "Appointments" WHERE "PatientId" = :patient_id AND "Status" = 2
);

-- An upcoming, confirmed appointment (re-created if the previous one has passed).
INSERT INTO "Appointments" ("AppointmentId", "PatientId", "TherapistId", "ClinicId", "TherapistName",
                            "Title", "AppointmentTime", "Status", "Notes", "IsDeleted", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), :patient_id, :therapist_id, :clinic_id, 'Demo Therapist',
       'Follow-up session', date_trunc('day', now()) + interval '3 days' + interval '8 hours',
       1 /* Confirmed */, 'Sample appointment for review.', false, now(), now()
WHERE NOT EXISTS (
  SELECT 1 FROM "Appointments"
  WHERE "PatientId" = :patient_id AND "Status" = 1 AND "AppointmentTime" > now()
);

-- Open booking slots so the reviewer can try requesting an appointment
-- (times are UTC; 07:00–09:00 UTC is 09:00–11:00 / 10:00–12:00 clinic time).
INSERT INTO "AppointmentSlots" ("Id", "ClinicId", "TherapistId", "ScheduledAt", "Status",
                                "IsDeleted", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), :clinic_id, :therapist_id, s.at, 0 /* Available */, false, now(), now()
FROM (
  VALUES (date_trunc('day', now()) + interval '4 days' + interval '7 hours'),
         (date_trunc('day', now()) + interval '5 days' + interval '8 hours'),
         (date_trunc('day', now()) + interval '6 days' + interval '9 hours')
) AS s(at)
WHERE (SELECT count(*) FROM "AppointmentSlots"
       WHERE "TherapistId" = :therapist_id AND "Status" = 0 AND "ScheduledAt" > now() AND NOT "IsDeleted") < 3;

COMMIT;

\echo 'Review demo account ready. Username: applereview'
SELECT (SELECT count(*) FROM "ExerciseAssignments" WHERE "PatientId" = :patient_id AND NOT "IsDeleted") AS exercises,
       (SELECT count(*) FROM "Appointments" WHERE "PatientId" = :patient_id AND NOT "IsDeleted")        AS appointments,
       (SELECT count(*) FROM "AppointmentSlots" WHERE "TherapistId" = :therapist_id
          AND "Status" = 0 AND "ScheduledAt" > now() AND NOT "IsDeleted")                              AS open_slots;
