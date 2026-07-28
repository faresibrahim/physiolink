using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.Controllers;

[Authorize]
public class TherapistsController : BaseController
{
    private readonly ApiClient _apiClient;

    public TherapistsController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET /Therapists?page=1
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        //if there are no therapists, the call will return an empty list, not null
        var therapists = await _apiClient.GetTherapistsAsync(page, 10);
        if (therapists == null)
        {
            TempData["ErrorMessage"] = "Failed to load therapists. Please try again.";
            return RedirectToAction("Index", "Dashboard");
        }

        var filters = new Dictionary<string, string>();
        

        var viewModel = new TherapistListViewModel
        {
            Therapists = therapists.Items,
            TotalCount = therapists.TotalCount,
            TotalPages = therapists.TotalPages,
            CurrentPage = page,
            Pagination = new PaginationViewModel
            {
                TotalPages = therapists.TotalPages,
                CurrentPage = page,
                Filters = filters,
            }
        };

        return View(viewModel);
    }

    // GET /Therapists/Detail/{id}
    public async Task<IActionResult> Detail(Guid id)
    {
        var therapistDetails = await _apiClient.GetTherapistByIdAsync(id);
        if (therapistDetails == null)
        {
            TempData["ErrorMessage"] = "The details for this therapist don't exist";
            return RedirectToAction("Index", "Therapists");
        }
        var patientsTask   = _apiClient.GetPatientsAsync(1, 100, id);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);
        await Task.WhenAll(patientsTask, therapistsTask);

        var patients   = await patientsTask;
        var therapists = await therapistsTask;

        var viewModel = new TherapistDetailViewModel
        {
            Therapist = therapistDetails,
            AssignedPatients = patients?.Items ?? [],
            Therapists = therapists?.Items ?? []
        };

        return View(viewModel);
    }

    // GET /Therapists/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TherapistFormViewModel { IsEdit = false });
    }

    // POST /Therapists/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TherapistFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var request = new CreateTherapistRequest
        {
            FirstName   = model.FirstName,
            LastName    = model.LastName,
            Email       = model.Email,
            PhoneNumber = model.PhoneNumber,
            Speciality  = model.Specialty
        };

        var success = await _apiClient.CreateTherapistAsync(request);
        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to create therapist. Please try again.";
            return View(model);
        }

        TempData["SuccessMessage"] = "Therapist created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Therapists/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var therapist = await _apiClient.GetTherapistByIdAsync(id);
        if (therapist == null)
        {
            TempData["ErrorMessage"] = "Therapist not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new TherapistFormViewModel
        {
            TherapistId = therapist.Id,
            FirstName   = therapist.FirstName,
            LastName    = therapist.LastName,
            Email       = therapist.Email,
            PhoneNumber = therapist.PhoneNumber,
            Specialty   = therapist.Specialty,
            IsEdit      = true
        };

        return View(viewModel);
    }

    // POST /Therapists/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TherapistFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.IsEdit = true;
            return View(model);
        }

        var request = new UpdateTherapistRequest
        {
            FirstName   = model.FirstName,
            LastName    = model.LastName,
            Email       = model.Email,
            PhoneNumber = model.PhoneNumber,
            Speciality  = model.Specialty
        };

        var success = await _apiClient.UpdateTherapistAsync(id, request);
        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to update therapist. Please try again.";
            return View(model);
        }

        TempData["SuccessMessage"] = "Therapist updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Therapists/Deactivate/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _apiClient.DeactivateTherapistAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Therapist deactivated successfully."
            : "Failed to deactivate therapist. Please try again.";

        return RedirectToAction(nameof(Index));
    }
}