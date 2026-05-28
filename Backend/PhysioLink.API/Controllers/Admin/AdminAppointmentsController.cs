using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Appointments;
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
            var result = await _adminAppointmentService.CreateAsync(createAppointmentDto);
            return CreatedAtAction(nameof(GetById), new { id = result.AppointmentId }, result);
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
    }
}
