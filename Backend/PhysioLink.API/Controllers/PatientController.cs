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
