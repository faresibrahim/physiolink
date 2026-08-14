using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.Exceptions;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers.Admin
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1/admin/patients")]
    public class AdminPatientsController : ControllerBase
    {
        private readonly IAdminPatientService _adminPatientService;

        public AdminPatientsController(IAdminPatientService adminPatientService)
        {
            _adminPatientService = adminPatientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10, Guid? therapistId = null, [FromQuery] string? search = null)
        {
            var result = await _adminPatientService.GetAllAsync(page, pageSize, therapistId, search);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _adminPatientService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto createPatientDto)
        {
            try
            {
                var result = await _adminPatientService.CreateAsync(createPatientDto);
                return CreatedAtAction(nameof(GetById), new { id = result.PatientId }, result);
            }
            catch (EmailInUseException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDto updatePatientDto)
        {
            var result = await _adminPatientService.UpdateAsync(id, updatePatientDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _adminPatientService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
