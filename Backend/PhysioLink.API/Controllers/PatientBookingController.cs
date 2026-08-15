using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.API.Extensions;
using PhysioLink.Application.DTOs.Slots;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers
{
    // Slot-based booking for the signed-in patient (spec Phase 3 / 5). The patient is
    // always resolved from the JWT identity — never from the URL or body.
    [ApiController]
    [Route("api/v1/patients/me")]
    [Authorize(Roles = "Patient")]
    public class PatientBookingController : ControllerBase
    {
        private readonly IPatientSlotService _patientSlotService;

        public PatientBookingController(IPatientSlotService patientSlotService)
        {
            _patientSlotService = patientSlotService;
        }

        // GET /api/v1/patients/me/slots?from=&to=
        [HttpGet("slots")]
        public async Task<IActionResult> GetMySlots([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        {
            if (User.GetApplicationUserId() is not { } userId) return Unauthorized();
            var slots = await _patientSlotService.GetMySlotsAsync(userId, from, to, ct);
            return Ok(slots);
        }

        // POST /api/v1/patients/me/appointments  { slotId, type, notes }
        [HttpPost("appointments")]
        public async Task<IActionResult> RequestSlot([FromBody] RequestSlotDto request, CancellationToken ct)
        {
            if (User.GetApplicationUserId() is not { } userId) return Unauthorized();

            var result = await _patientSlotService.RequestSlotAsync(userId, request, ct);
            return result.Outcome switch
            {
                RequestSlotOutcome.Created => StatusCode(StatusCodes.Status201Created, result.Appointment),
                RequestSlotOutcome.PatientNotFound => NotFound(),
                // Lost the race, slot gone, not their therapist, in the past, or unassigned.
                RequestSlotOutcome.Conflict => Conflict("That slot is no longer available."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        // GET /api/v1/patients/me/appointments
        [HttpGet("appointments")]
        public async Task<IActionResult> GetMyAppointments(CancellationToken ct)
        {
            if (User.GetApplicationUserId() is not { } userId) return Unauthorized();
            var appointments = await _patientSlotService.GetMyAppointmentsAsync(userId, ct);
            return Ok(appointments);
        }
    }
}
