using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Assignments;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers.Admin
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1")]
    public class AdminAssignmentsController : ControllerBase
    {
        private readonly IAdminAssignmentService _adminAssignmentService;

        public AdminAssignmentsController(IAdminAssignmentService adminAssignmentService)
        {
            _adminAssignmentService = adminAssignmentService;
        }

        [HttpGet("admin/patients/{patientId:guid}/assignments")]
        public async Task<IActionResult> GetAllByPatient(Guid patientId, int page = 1, int pageSize = 10)
        {
            var result = await _adminAssignmentService.GetAllByPatientAsync(patientId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("admin/assignments/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _adminAssignmentService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("admin/patients/{patientId:guid}/assignments")]
        public async Task<IActionResult> Create(Guid patientId, [FromBody] CreateAssignmentDto createAssignmentDto)
        {
            var result = await _adminAssignmentService.CreateAsync(patientId, createAssignmentDto);
            return CreatedAtAction(nameof(GetById), new { id = result.ExerciseAssignmentId }, result);
        }

        [HttpPut("admin/assignments/{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssignmentDto updateAssignmentDto)
        {
            var result = await _adminAssignmentService.UpdateAsync(id, updateAssignmentDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("admin/assignments/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _adminAssignmentService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
