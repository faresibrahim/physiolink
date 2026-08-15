using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.API.Extensions;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers {

[ApiController]
[Route("api/v1")]
[Authorize(Roles = "Patient")]

public class ExerciseController : ControllerBase
    {
    private readonly IExerciseService _exerciseService;
    private readonly IPatientService _patientService;

    public ExerciseController(IExerciseService exerciseService, IPatientService patientService)
    {
        _exerciseService = exerciseService;
        _patientService = patientService;
    }

    [HttpGet("patients/{id}/exercises")]                //no DTOs means no validators
                                                        //default values kick in if the parameter is not sent by Flutter
    public async Task<IActionResult> GetPatientExercises(Guid id, int page = 1, int pageSize = 10)
    {
        // The patient is resolved from the JWT, not trusted from the URL: a patient
        // can only list their own exercises, never another patient's by GUID-swapping.
        if (await ResolveCallerPatientIdAsync() is not { } callerPatientId || callerPatientId != id)
            return Forbid();

        var result = await _exerciseService.GetPatientExercisesAsync(id, page, pageSize);
        return Ok(result);
    }

     [HttpPost("exercises/{id}/feedback")]
        public async Task<IActionResult> SubmitFeedbackLevel(Guid id, [FromBody] SubmitFeedbackDto level)
        {
            if (await ResolveCallerPatientIdAsync() is not { } callerPatientId)
                return Forbid();

            // Ownership is enforced in the repository: feedback on an assignment that
            // isn't the caller's returns false → 404, leaking nothing about it.
            var result = await _exerciseService.SubmitFeedbackAsync(id, level, callerPatientId);
            if(!result)
                return NotFound();
            return Ok();
        }

    private async Task<Guid?> ResolveCallerPatientIdAsync()
    {
        if (User.GetApplicationUserId() is not { } userId) return null;
        return await _patientService.ResolvePatientIdAsync(userId);
    }

    }

}
