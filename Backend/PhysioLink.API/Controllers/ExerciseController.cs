using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers {

[ApiController]
[Route("api/v1")]
[Authorize(Roles = "Patient")]

public class ExerciseController : ControllerBase
    {
    private readonly IExerciseService _exerciseService;

    public ExerciseController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    [HttpGet("patients/{id}/exercises")]                //no DTOs means no validators
                                                        //default values kick in if the parameter is not sent by Flutter
    public async Task<IActionResult> GetPatientExercises(Guid id, int page = 1, int pageSize = 10)
    {
        var result = await _exerciseService.GetPatientExercisesAsync(id, page, pageSize);
        return Ok(result);
    }
    
     [HttpPost("exercises/{id}/feedback")]
        public async Task<IActionResult> SubmitFeedbackLevel(Guid id, [FromBody] SubmitFeedbackDto level)
        {
            var result = await _exerciseService.SubmitFeedbackAsync(id, level);
            if(!result) 
                return NotFound();
            return Ok();
        }


    }

}
