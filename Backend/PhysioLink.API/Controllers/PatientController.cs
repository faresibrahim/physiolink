using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var result = await _patientService.GetPatientProfileAsync(id);
            return Ok(result);
        }

        [HttpPut("patients/{id}/profile")]
        public async Task<IActionResult> UpdatePatientProfile(Guid id, [FromBody]UpdatePatientProfileDto request)
     {
        var success = await _patientService.UpdatePatientProfileAsync(id, request);
         if (!success){
          return NotFound();
         }
         return NoContent();
     }
       [HttpGet("patients/{id}/progress")]
       public async Task<IActionResult> GetPatientProgress(Guid id)
        {
            var result = await _patientService.GetPatientProgressAsync(id);
            if(result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }



    }



}