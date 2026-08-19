using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.Controllers;

[Authorize]
public class PatientsController : BaseController
{
    private readonly ApiClient _apiClient;

    public PatientsController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET /Patients?page=1&searchQuery=&therapistId=
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, string? searchQuery = null, Guid? therapistId = null)
    {
        var patientsTask   = _apiClient.GetPatientsAsync(page, 10, therapistId, searchQuery);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);

        await Task.WhenAll(patientsTask, therapistsTask);

        var patients   = await patientsTask;
        var therapists = await therapistsTask;

        if (patients == null)
        {
            TempData["ErrorMessage"] = "Failed to load patients. Please try again.";
            return RedirectToAction("Index", "Dashboard");
        }



        var filters = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(searchQuery))
            filters["searchQuery"] = searchQuery;
        if (therapistId.HasValue)
            filters["therapistId"] = therapistId.Value.ToString();


        var viewModel = new PatientListViewModel
        {
            Patients = patients.Items,
            Therapists = therapists?.Items ?? [],
            TotalCount = patients.TotalCount,
            TotalPages = patients.TotalPages,
            CurrentPage = page,
            SearchQuery = searchQuery,
            SelectedTherapistId = therapistId,
            Pagination = new PaginationViewModel
            {
                TotalPages = patients.TotalPages,
                CurrentPage = page,
                Filters = filters
            }
        }; 

        return View(viewModel);
    }

    // GET /Patients/Detail/{id}
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        var patientTask     = _apiClient.GetPatientByIdAsync(id);
        var therapistsTask  = _apiClient.GetTherapistsAsync(1, 100);
        var exercisesTask   = _apiClient.GetExercisesAsync();
        var attachmentsTask = _apiClient.GetPatientAttachmentsAsync(id);

        // Allow exercises/attachments to fail without taking down the whole page
        List<ExerciseResponse> exercises;
        List<PatientAttachmentResponse> attachments;
        try
        {
            await Task.WhenAll(patientTask, therapistsTask, exercisesTask, attachmentsTask);
            exercises   = exercisesTask.Result ?? [];
            attachments = attachmentsTask.Result ?? [];
        }
        catch
        {
            // If a side list threw, grab what we can
            exercises   = exercisesTask.IsCompletedSuccessfully ? exercisesTask.Result ?? [] : [];
            attachments = attachmentsTask.IsCompletedSuccessfully ? attachmentsTask.Result ?? [] : [];
        }

        var patient    = patientTask.IsCompletedSuccessfully ? patientTask.Result : null;
        var therapists = therapistsTask.IsCompletedSuccessfully ? therapistsTask.Result : null;

        if (patient == null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        // Resolve the currently-assigned therapist's ID by matching name, so the
        // Edit Patient dialog can preselect it (PatientDetailResponse only carries the name).
        var assignedTherapist = therapists?.Items.FirstOrDefault(t =>
            $"{t.FirstName} {t.LastName}" == patient.TherapistName);

        var viewModel = new PatientDetailViewModel
        {
            Patient            = patient,
            Therapists         = therapists?.Items ?? [],
            Exercises          = exercises,
            Attachments        = attachments,
            PatientTherapistId = assignedTherapist?.Id
        };

        return View(viewModel);
    }

    // POST /Patients/UploadAttachment
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(12 * 1024 * 1024)] // headroom over the 10 MB file cap for multipart overhead
    public async Task<IActionResult> UploadAttachment(Guid patientId, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Choose a file to upload.";
            return RedirectToAction(nameof(Detail), new { id = patientId });
        }

        var (success, error) = await _apiClient.UploadPatientAttachmentAsync(patientId, file);
        if (success)
            TempData["SuccessMessage"] = $"Uploaded \"{file.FileName}\".";
        else
            TempData["ErrorMessage"] = error ?? "Upload failed. Please try again.";

        return RedirectToAction(nameof(Detail), new { id = patientId });
    }

    // GET /Patients/ViewAttachment?patientId=&attachmentId= — opened by clicking the
    // filename. Same bytes as DownloadAttachment, but with an "inline" disposition so
    // a browser-renderable file (PDF, image, text) opens in the tab instead of saving.
    // Anything the browser can't render (e.g. .docx) still falls back to a download.
    [HttpGet]
    public async Task<IActionResult> ViewAttachment(Guid patientId, Guid attachmentId)
    {
        var result = await _apiClient.DownloadPatientAttachmentAsync(patientId, attachmentId);
        if (result == null)
        {
            TempData["ErrorMessage"] = "That attachment could not be found.";
            return RedirectToAction(nameof(Detail), new { id = patientId });
        }

        var (bytes, contentType, fileName) = result.Value;
        Response.Headers.ContentDisposition =
            new System.Net.Mime.ContentDisposition { Inline = true, FileName = fileName }.ToString();
        return File(bytes, contentType);
    }

    // GET /Patients/DownloadAttachment?patientId=&attachmentId=
    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(Guid patientId, Guid attachmentId)
    {
        var result = await _apiClient.DownloadPatientAttachmentAsync(patientId, attachmentId);
        if (result == null)
        {
            TempData["ErrorMessage"] = "That attachment could not be found.";
            return RedirectToAction(nameof(Detail), new { id = patientId });
        }

        var (bytes, contentType, fileName) = result.Value;
        return File(bytes, contentType, fileName);
    }

    // POST /Patients/DeleteAttachment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(Guid patientId, Guid attachmentId)
    {
        var ok = await _apiClient.DeletePatientAttachmentAsync(patientId, attachmentId);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
            ok ? "Attachment removed." : "Could not remove the attachment.";
        return RedirectToAction(nameof(Detail), new { id = patientId });
    }
    // GET /Patients/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var patientTask    = _apiClient.GetPatientByIdAsync(id);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);
        await Task.WhenAll(patientTask, therapistsTask);

        var patient    = await patientTask;
        var therapists = await therapistsTask;

        if (patient == null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        // Resolve the currently-assigned therapist's ID from the therapist list by matching name
        var assignedTherapist = therapists?.Items.FirstOrDefault(t =>
            $"{t.FirstName} {t.LastName}" == patient.TherapistName);

        var viewModel = new PatientFormViewModel
        {
            PatientId   = patient.PatientId,
            FirstName   = patient.FirstName,
            LastName    = patient.LastName,
            Email       = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            Diagnosis   = patient.Diagnosis,
            IsActive    = patient.IsActive,
            TherapistId = assignedTherapist?.Id,
            IsEdit      = true,
            Therapists  = therapists?.Items ?? []
        };

        return View(viewModel);
    }

    // GET /Patients/GetById/{id}
    // Backs the Edit Patient dialog on the Patients list, where each row's
    // full data isn't already on the page (unlike the Detail page, which
    // server-renders its own edit modal directly from the loaded patient).
    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    {
        var patientTask    = _apiClient.GetPatientByIdAsync(id);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);
        await Task.WhenAll(patientTask, therapistsTask);

        var patient = await patientTask;
        if (patient == null) return Json(new { error = "Not found" });

        var therapists = await therapistsTask;
        var assignedTherapist = therapists?.Items.FirstOrDefault(t =>
            $"{t.FirstName} {t.LastName}" == patient.TherapistName);

        return Json(new
        {
            patientId   = patient.PatientId,
            firstName   = patient.FirstName,
            lastName    = patient.LastName,
            email       = patient.Email,
            phoneNumber = patient.PhoneNumber,
            diagnosis   = patient.Diagnosis,
            therapistId = assignedTherapist?.Id,
            isActive    = patient.IsActive
        });
    }

    // POST /Patients/Edit/{id}
    // Submitted by the Edit Patient dialog (Patient Detail page), so failures
    // redirect back to Detail with a TempData message rather than re-rendering
    // a form view — there's no full Edit page to redisplay with inline errors.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PatientFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var request = new UpdatePatientRequest
        {
            FirstName   = model.FirstName,
            LastName    = model.LastName,
            Email       = model.Email,
            PhoneNumber = model.PhoneNumber,
            Diagnosis   = model.Diagnosis,
            IsActive    = model.IsActive,
            TherapistId = model.TherapistId
        };

        var success = await _apiClient.UpdatePatientAsync(id, request);
        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to update patient. Please try again.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        TempData["SuccessMessage"] = "Patient updated successfully.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // POST /Patients/Deactivate/{id}
    // Destructive, so the therapist re-enters their own account password to confirm.
    // Failures return to Detail — the patient is still there and the user stays in context.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            TempData["ErrorMessage"] = "Enter your account password to confirm deactivation.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        if (!await _apiClient.VerifyPasswordAsync(password))
        {
            TempData["ErrorMessage"] = "Incorrect password. The patient was not deactivated.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var success = await _apiClient.DeactivatePatientAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to deactivate patient. Please try again.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        TempData["SuccessMessage"] = "Patient deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Patients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(Index));
        }

        var patient = new CreatePatientRequest
        {
            FirstName   = model.FirstName,
            LastName    = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Diagnosis   = model.Diagnosis,
            Email       = model.Email,
            TherapistId = model.TherapistId
        };

        var result = await _apiClient.CreatePatientAsync(patient);
        if (result == null)
        {
            TempData["ErrorMessage"] = "Failed to create patient. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        TempData["PatientCreatedName"]     = $"{result.FirstName} {result.LastName}";
        TempData["PatientCreatedEmail"]    = result.Email;
        TempData["PatientCreatedPassword"] = result.TemporaryPassword ?? string.Empty;
        return RedirectToAction(nameof(Index));
    }


}
