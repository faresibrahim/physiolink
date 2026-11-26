using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.API.Extensions;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers
{

    [ApiController]
    [Route("api/v1")]
    [Authorize(Roles = "Patient")]

    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("patients/{id}/profile")]
        public async Task<IActionResult> GetPatientProfile(Guid id)
        {
            if (!await CallerOwnsPatientAsync(id)) return Forbid();
            var result = await _patientService.GetPatientProfileAsync(id);
            return Ok(result);
        }

        [HttpPut("patients/{id}/profile")]
        public async Task<IActionResult> UpdatePatientProfile(Guid id, [FromBody]UpdatePatientProfileDto request)
     {
        if (!await CallerOwnsPatientAsync(id)) return Forbid();
        var success = await _patientService.UpdatePatientProfileAsync(id, request);
         if (!success){
          return NotFound();
         }
         return NoContent();
     }
       [HttpGet("patients/{id}/progress")]
       public async Task<IActionResult> GetPatientProgress(Guid id)
        {
            if (!await CallerOwnsPatientAsync(id)) return Forbid();
            var result = await _patientService.GetPatientProgressAsync(id);
            if(result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // No {id} route param — the patient is identified purely from the JWT,
        // same as CallerOwnsPatientAsync resolves it, so there's no GUID here for
        // an IDOR to swap in the first place.
        [HttpPatch("patients/me/device-token")]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenDto request)
        {
            if (User.GetApplicationUserId() is not { } userId) return Forbid();
            var patientId = await _patientService.ResolvePatientIdAsync(userId);
            if (patientId is not { } pid) return Forbid();

            var success = await _patientService.RegisterDeviceTokenAsync(pid, request.DeviceToken);
            if (!success) return NotFound();
            return NoContent();
        }

        // Daily exercise reminder. Like device-token, the patient comes only from the
        // JWT. A patient who never set one gets the defaults (disabled, 18:00), not 404.
        [HttpGet("patients/me/reminder")]
        public async Task<IActionResult> GetReminder()
        {
            if (User.GetApplicationUserId() is not { } userId) return Forbid();
            var patientId = await _patientService.ResolvePatientIdAsync(userId);
            if (patientId is not { } pid) return Forbid();

            var reminder = await _patientService.GetReminderAsync(pid);
            if (reminder is null) return NotFound();
            return Ok(reminder);
        }

        // Time is "HH:mm" in clinic local time; UpdateReminderValidator returns 400
        // for anything else.
        [HttpPut("patients/me/reminder")]
        public async Task<IActionResult> UpdateReminder([FromBody] UpdateReminderDto request)
        {
            if (User.GetApplicationUserId() is not { } userId) return Forbid();
            var patientId = await _patientService.ResolvePatientIdAsync(userId);
            if (patientId is not { } pid) return Forbid();

            var saved = await _patientService.UpdateReminderAsync(pid, request);
            if (saved is null) return NotFound();
            return Ok(saved);
        }

        // Ensures the {id} in the route is the caller's own PatientId. The patient is
        // resolved from the JWT identity — never trusted from the URL — closing the
        // IDOR where any patient could read/edit another's record by swapping the GUID.
        private async Task<bool> CallerOwnsPatientAsync(Guid patientId)
        {
            if (User.GetApplicationUserId() is not { } userId) return false;
            var callerPatientId = await _patientService.ResolvePatientIdAsync(userId);
            return callerPatientId is { } cpid && cpid == patientId;
        }
    }



}
