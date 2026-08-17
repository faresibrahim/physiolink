using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.Exceptions;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers.Admin
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1/admin/appointments")]
    public class AdminAppointmentsController : ControllerBase
    {
        private readonly IAdminAppointmentService _adminAppointmentService;

        public AdminAppointmentsController(IAdminAppointmentService adminAppointmentService)
        {
            _adminAppointmentService = adminAppointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 10,
            DateTime? from = null,
            DateTime? to = null,
            string? status = null)
        {
            var result = await _adminAppointmentService.GetAllAsync(page, pageSize, from, to, status);
            return Ok(result);
        }

        // GET /api/v1/admin/appointments/history?page=&pageSize=&status=&therapistName=
        // Archive of past-due appointments (Completed / Rejected / Expired / Cancelled).
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            int page = 1,
            int pageSize = 20,
            string? status = null,
            string? therapistName = null)
        {
            var result = await _adminAppointmentService.GetHistoryAsync(page, pageSize, status, therapistName);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _adminAppointmentService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto createAppointmentDto)
        {
            try
            {
                var result = await _adminAppointmentService.CreateAsync(createAppointmentDto);
                return CreatedAtAction(nameof(GetById), new { id = result.AppointmentId }, result);
            }
            catch (SlotConflictException ex)
            {
                // The chosen slot was claimed between fetch and submit.
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentDto updateAppointmentDto)
        {
            var result = await _adminAppointmentService.UpdateAsync(id, updateAppointmentDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _adminAppointmentService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // GET /api/v1/admin/appointments/requests?therapistId=
        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests([FromQuery] Guid? therapistId, CancellationToken ct)
        {
            var result = await _adminAppointmentService.GetRequestsAsync(therapistId, ct);
            return Ok(result);
        }

        // PUT /api/v1/admin/appointments/{id}/accept
        [HttpPut("{id:guid}/accept")]
        public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
            => Map(await _adminAppointmentService.AcceptAsync(id, ct));

        // PUT /api/v1/admin/appointments/{id}/reject
        [HttpPut("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
            => Map(await _adminAppointmentService.RejectAsync(id, ct));

        // PUT /api/v1/admin/appointments/{id}/cancel
        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
            => Map(await _adminAppointmentService.CancelAsync(id, ct));

        private IActionResult Map(AppointmentActionOutcome outcome) => outcome switch
        {
            AppointmentActionOutcome.Ok => NoContent(),
            AppointmentActionOutcome.NotFound => NotFound(),
            AppointmentActionOutcome.InvalidState => Conflict("Appointment is not in a state that allows this action."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
