using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Exercises;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers.Admin
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1/admin/exercises")]
    public class AdminExercisesController : ControllerBase
    {
        private readonly IAdminExerciseService _adminExerciseService;

        public AdminExercisesController(IAdminExerciseService adminExerciseService)
        {
            _adminExerciseService = adminExerciseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null, [FromQuery] string? difficulty = null, [FromQuery] string? category = null)
        {
            var result = await _adminExerciseService.GetAllAsync(search, difficulty, category);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _adminExerciseService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdminExerciseDto createAdminExerciseDto)
        {
            var result = await _adminExerciseService.CreateAsync(createAdminExerciseDto);
            return Created(string.Empty, result);
        }
    }
}
