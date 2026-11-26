-- =============================================================================
-- Permanently erase a patient's account and data (privacy deletion requests)
-- =============================================================================
-- The admin panel only SOFT-deletes patients (IsDeleted = true); the rows stay.
-- The privacy policy promises permanent erasure within 30 days of a request, so
-- run this script for every verified deletion request.
--
-- It removes, for every patient row with the given username (including
-- soft-deleted duplicates):
--   exercise assignments (+ ratings), appointments, attached documents,
--   the patient record and the login account.
-- Any FUTURE booking slot the patient was holding is released back to Available.
--
-- Usage (dry run first — prints what would be erased, then rolls back):
--   psql "<connection string>" -v username='someuser' -v confirm=no  -f Backend/Scripts/erase_patient.sql
-- Then for real:
--   psql "<connection string>" -v username='someuser' -v confirm=yes -f Backend/Scripts/erase_patient.sql
--
-- Before running: verify the request came from the account holder (username +
-- registered phone number match), and note the date for your records.
-- =============================================================================

\set ON_ERROR_STOP on

\if :{?username}
\else
  \echo 'Missing -v username=...'
  \quit
\endif
\if :{?confirm}
\else
  \set confirm no
\endif

BEGIN;

SELECT set_config('erase.username', :'username', true);

CREATE TEMP TABLE erase_targets ON COMMIT DROP AS
SELECT p."PatientId", p."ApplicationUserId", p."FirstName", p."LastName", p."IsDeleted"
FROM "Patients" p
WHERE lower(p."Username") = lower(current_setting('erase.username'))
  AND current_setting('erase.username') <> '';

\echo 'Patients matching this username:'
SELECT * FROM erase_targets;

DO $$
DECLARE
  n_targets int;
  n int;
BEGIN
  SELECT count(*) INTO n_targets FROM erase_targets;
  IF n_targets = 0 THEN
    RAISE EXCEPTION 'No patient with username "%" — nothing erased.', current_setting('erase.username');
  END IF;

  UPDATE "AppointmentSlots" s SET "Status" = 0, "UpdatedAt" = now()
  WHERE s."ScheduledAt" > now()
    AND s."Id" IN (SELECT a."AppointmentSlotId" FROM "Appointments" a
                   WHERE a."PatientId" IN (SELECT "PatientId" FROM erase_targets)
                     AND a."AppointmentSlotId" IS NOT NULL);
  GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'Slots released:        %', n;

  DELETE FROM "ExerciseAssignments" WHERE "PatientId" IN (SELECT "PatientId" FROM erase_targets);
  GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'Exercise assignments:  %', n;

  DELETE FROM "Appointments" WHERE "PatientId" IN (SELECT "PatientId" FROM erase_targets);
  GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'Appointments:          %', n;

  DELETE FROM "PatientAttachments" WHERE "PatientId" IN (SELECT "PatientId" FROM erase_targets);
  GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'Attachments:           %', n;

  DELETE FROM "Patients" WHERE "PatientId" IN (SELECT "PatientId" FROM erase_targets);
  GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'Patient records:       %', n;

  DELETE FROM "Users" WHERE "ApplicationUserId" IN (SELECT "ApplicationUserId" FROM erase_targets);
  GET DIAGNOSTICS n = ROW_COUNT; RAISE NOTICE 'Login accounts:        %', n;
END $$;

\if :confirm
  COMMIT;
  \echo 'ERASED permanently.'
\else
  ROLLBACK;
  \echo 'Dry run only — nothing was changed. Re-run with -v confirm=yes to erase.'
\endif
