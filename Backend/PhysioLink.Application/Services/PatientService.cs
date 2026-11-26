using PhysioLink.Application.Common;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.DTOs.Profile;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Application.Services
{
    public class PatientService : IPatientService
    {

        private readonly IPatientRepository _patientRepository;
        private readonly TimeProvider _timeProvider;

        // TimeProvider is optional so DI needs no extra registration (falls back to
        // the system clock); tests pass a fixed clock.
        public PatientService(IPatientRepository patientRepository, TimeProvider? timeProvider = null)
        {
            _patientRepository = patientRepository;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        public async Task<PatientProfileDto> GetPatientProfileAsync(Guid patientId)
        {
            return await _patientRepository.GetPatientProfileAsync(patientId);
        }

        public async Task<PatientProgressDto> GetPatientProgressAsync(Guid patientId)
        {
            return await _patientRepository.GetPatientProgressAsync(patientId);
        }

        public async Task<bool> UpdatePatientProfileAsync(Guid patientId, UpdatePatientProfileDto request)
        {
    return await _patientRepository.UpdatePatientProfileAsync(patientId, request);
        }

        public async Task<Guid?> ResolvePatientIdAsync(Guid applicationUserId)
        {
            var patient = await _patientRepository.GetPatientByUserIdAsync(applicationUserId);
            return patient?.PatientId;
        }

        public async Task<bool> RegisterDeviceTokenAsync(Guid patientId, string deviceToken)
        {
            return await _patientRepository.UpdateDeviceTokenAsync(patientId, deviceToken);
        }

        public async Task<ReminderDto?> GetReminderAsync(Guid patientId)
        {
            return await _patientRepository.GetReminderAsync(patientId);
        }

        public async Task<ReminderDto?> UpdateReminderAsync(Guid patientId, UpdateReminderDto request)
        {
            // UpdateReminderValidator rejects bad input with a 400 before we get here;
            // this guards any caller that skips the MVC pipeline.
            if (!ReminderTimeFormat.TryParse(request.Time, out var time))
                throw new ArgumentException("Time must be a 24-hour HH:mm value.", nameof(request));

            // If the chosen time has already passed today (clinic-local wall clock),
            // count today as handled so enabling at 20:00 for 18:00 — or re-enabling
            // after the time passed — doesn't fire a stale reminder on the next tick.
            // The first reminder is then tomorrow.
            var clinicNow = ClinicTime.ToLocal(_timeProvider.GetUtcNow().UtcDateTime);
            DateOnly? markHandledOn = request.Enabled && time <= TimeOnly.FromDateTime(clinicNow)
                ? DateOnly.FromDateTime(clinicNow)
                : null;

            var updated = await _patientRepository.UpdateReminderAsync(patientId, request.Enabled, time, markHandledOn);
            if (!updated)
                return null;

            return new ReminderDto
            {
                Enabled = request.Enabled,
                Time = ReminderTimeFormat.Format(time),
            };
        }
    }
}
