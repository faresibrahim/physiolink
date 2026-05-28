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
        var patientTask    = _apiClient.GetPatientByIdAsync(id);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);
        var exercisesTask  = _apiClient.GetExercisesAsync();

        // Allow exercises to fail without taking down the whole page
        List<ExerciseResponse> exercises;
        try
        {
            await Task.WhenAll(patientTask, therapistsTask, exercisesTask);
            exercises = exercisesTask.Result ?? [];
        }
        catch
        {
            // If exercises threw, grab what we can
            exercises = exercisesTask.IsCompletedSuccessfully ? exercisesTask.Result ?? [] : [];
        }

        var patient    = patientTask.IsCompletedSuccessfully ? patientTask.Result : null;
        var therapists = therapistsTask.IsCompletedSuccessfully ? therapistsTask.Result : null;

        if (patient == null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new PatientDetailViewModel
        {
            Patient    = patient,
            Therapists = therapists?.Items ?? [],
            Exercises  = exercises
        };

        return View(viewModel);
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

    // POST /Patients/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PatientFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.IsEdit     = true;
            model.Therapists = (await _apiClient.GetTherapistsAsync(1, 100))?.Items ?? [];
            return View(model);
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
            model.IsEdit     = true;
            model.Therapists = (await _apiClient.GetTherapistsAsync(1, 100))?.Items ?? [];
            return View(model);
        }

        TempData["SuccessMessage"] = "Patient updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Patients/Deactivate/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _apiClient.DeactivatePatientAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Patient deactivated successfully."
            : "Failed to deactivate patient. Please try again.";

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
