using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Therapists;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1")]
    public class TherapistController : ControllerBase
    {
        private readonly IAdminTherapistService _adminTherapistService;
        public TherapistController(IAdminTherapistService adminTherapistService)
        {
            _adminTherapistService = adminTherapistService;
        }

        [HttpGet("admin/therapists")]
        public async Task<IActionResult> GetAllTherapists(int page = 1, int pageSize = 10)
        {
            var result = await _adminTherapistService.GetAllAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("admin/therapists/{id}")]
        public async Task<IActionResult> GetTherapistById (Guid id)
        {
            var result = await _adminTherapistService.GetByIdAsync(id);
            if (result == null ) return NotFound();
            return Ok(result);
        }

        [HttpPost("admin/therapists")]
        public async Task<IActionResult> CreateTherapist([FromBody] CreateTherapistDto createTherapistDto)
        {
            var result = await _adminTherapistService.CreateAsync(createTherapistDto);
            return CreatedAtAction(nameof(GetTherapistById), new { id = result.TherapistId }, result);
        }

        [HttpPut("admin/therapists/{id}")]
        public async Task<IActionResult> UpdateTherapist(Guid id, [FromBody] UpdateTherapistDto updateTherapistDto)
        {
            var result = await _adminTherapistService.UpdateAsync(id, updateTherapistDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("admin/therapists/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await _adminTherapistService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }


    }



}