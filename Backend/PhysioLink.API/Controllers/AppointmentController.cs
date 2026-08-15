
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.API.Extensions;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Authorize(Roles = "Patient")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        public AppointmentController(IAppointmentService appointmentService, IPatientService patientService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
        }

        [HttpGet("patients/{id}/appointments")]
        public async Task<IActionResult> GetPatientAppointments(Guid id, int page = 1, int pageSize = 10)
        {
            // Resolve the patient from the JWT — a patient may only read their own
            // appointments, not another's by swapping the {id} in the URL.
            if (await ResolveCallerPatientIdAsync() is not { } callerPatientId || callerPatientId != id)
                return Forbid();

            var result = await _appointmentService.GetPatientAppointmentAsync(id, page, pageSize);
            return Ok(result);
        }

        [HttpPost("appointments")]
        public async Task<IActionResult> CreateAppointment(AppointmentRequestDto request)
        {
            // Bind the appointment to the authenticated caller, ignoring any PatientId
            // supplied in the body — otherwise a patient could book in someone else's name.
            if (await ResolveCallerPatientIdAsync() is not { } callerPatientId)
                return Forbid();

            request.PatientId = callerPatientId;
            var result = await _appointmentService.CreateAppointmentAsync(request);
            return CreatedAtAction(nameof(GetPatientAppointments), new { id = result.AppointmentId }, result);
        }

        private async Task<Guid?> ResolveCallerPatientIdAsync()
        {
            if (User.GetApplicationUserId() is not { } userId) return null;
            return await _patientService.ResolvePatientIdAsync(userId);
        }
    }
}
