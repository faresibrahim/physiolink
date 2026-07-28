using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels;
using System.Reflection;

namespace PhysioLink.AdminPanel.Controllers
{
    [Authorize]
    public class AssignmentController : BaseController
    {
        private readonly ApiClient _apiClient;
        public AssignmentController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentExerciseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction("Detail", "Patients", new { id = model.PatientId });
            }

            var assignment = new AssignExerciseRequest
            {
                ExerciseId        = model.ExerciseId,
                Sets              = model.Sets,
                Reps              = model.Reps,
                DurationMinutes   = model.DurationMinutes,
                ScheduledDate     = model.ScheduledDate,
                FrequencyPerWeek  = model.FrequencyPerWeek,
                TherapistName     = string.IsNullOrWhiteSpace(model.TherapistName) ? "Unassigned" : model.TherapistName,
            };

            var success = await _apiClient.AssignExerciseAsync(model.PatientId, assignment);

            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to assign exercise. Please try again.";
                return RedirectToAction("Detail", "Patients", new { id = model.PatientId });
            }

            TempData["SuccessMessage"] = "Exercise assigned successfully.";
            return RedirectToAction("Detail", "Patients", new { id = model.PatientId });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var assignment = await _apiClient.GetAssignmentByIdAsync(id);
            if (assignment == null) return Json(new { error = "Not found" });
            return Json(assignment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit (EditAssignmentViewModel model)
        {
            if(!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction("Detail", "Patients", new { id = model.PatientId });
            }
            
           
            var request = new UpdateAssignmentRequest
            {
                TherapistName = model.TherapistName,
                Sets = model.Sets,
                Reps = model.Reps,
                DurationMinutes = model.DurationMinutes,
                ScheduledDate = model.ScheduledDate,    
                FrequencyPerWeek = model.FrequencyPerWeek,
                Status = model.Status,
            };

            var result = await _apiClient.UpdateAssignmentAsync(model.ExerciseAssignmentId, request);

            if(!result)
            {
                TempData["ErrorMessage"] = "Couldn't update the assigned exercise.";
                return RedirectToAction("Detail", "Patients", new {id = model.PatientId});
            }
            TempData["SuccessMessage"] = "Assignment updated successfully.";
            return RedirectToAction("Detail", "Patients", new { id = model.PatientId });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(Guid id, Guid patientId)
        {
            var success = await _apiClient.DeleteAssignmentAsync(id);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Exercise unassigned successfully."
                : "Failed to unassign exercise. Please try again.";

            return RedirectToAction("Detail", "Patients", new { id = patientId });
        }
    }
}
