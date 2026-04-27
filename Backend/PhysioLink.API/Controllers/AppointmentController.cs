
using Microsoft.AspNetCore.Mvc;

using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.Interfaces;


namespace PhysioLink.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("patients/{id}/appointments")]
         public async Task<IActionResult> GetPatientAppointments(Guid id, int page = 1, int pageSize = 10)
    {
        var result = await _appointmentService.GetPatientAppointmentAsync(id, page, pageSize);
        return Ok(result);
    }

        [HttpPost("appointments")]
        public async Task<IActionResult> CreateAppointment(AppointmentRequestDto request)
        {
            var result = await _appointmentService.CreateAppointmentAsync(request);
            return CreatedAtAction(nameof(GetPatientAppointments), new { id = result.AppointmentId}, result);
        }

    }
}   