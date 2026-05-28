using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.Controllers;

[Authorize]
public class AppointmentsController : BaseController
{
    private readonly ApiClient _apiClient;

    public AppointmentsController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET /Appointment?weekStart=&page=1&status=&therapistFilter=&viewMode=list
    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? weekStart = null,
        int page = 1,
        string? status = null,
        string? therapistFilter = null,
        string viewMode = "list")
    {
        var today = DateTime.UtcNow.Date;
        var daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
        var computedWeekStart = weekStart ?? today.AddDays(-daysFromMonday);

        var from = computedWeekStart;
        var to = computedWeekStart.AddDays(7);

        var appointmentsTask = _apiClient.GetAppointmentsAsync(page, 50, status: status, from: from, to: to);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);
        var patientsTask = _apiClient.GetPatientsAsync(1, 200, null);

        await Task.WhenAll(appointmentsTask, therapistsTask, patientsTask);

        var appointments = await appointmentsTask;
        var therapists = await therapistsTask;
        var patients = await patientsTask;

        if (appointments == null)
        {
            TempData["ErrorMessage"] = "Failed to load appointments. Please try again.";
            return RedirectToAction("Index", "Dashboard");
        }

        var filters = new Dictionary<string, string>
        {
            ["weekStart"]      = computedWeekStart.ToString("yyyy-MM-dd"),
            ["viewMode"]       = viewMode,
        };
        if (!string.IsNullOrWhiteSpace(status))          filters["status"]          = status;
        if (!string.IsNullOrWhiteSpace(therapistFilter)) filters["therapistFilter"] = therapistFilter;

        var viewModel = new AppointmentListViewModel
        {
            Appointments = appointments.Items,
            CurrentPage = page,
            TotalPages = appointments.TotalPages,
            TotalAppointmentCount = appointments.TotalCount,
            WeekStart = computedWeekStart,
            StatusFilter = status,
            TherapistFilter = therapistFilter,
            ViewMode = viewMode,
            Therapists = therapists?.Items ?? [],
            Patients = patients?.Items ?? [],
            Pagination = new PaginationViewModel
            {
                TotalPages  = appointments.TotalPages,
                CurrentPage = page,
                Filters     = filters,
            },
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction("Index", "Appointments");
        }

        var appointment = new CreateAppointmentRequest
        {
            PatientId = model.PatientId,
            TherapistName = model.TherapistName,
            Title = model.Title,
            Notes = model.Notes,
            AppointmentTime = model.AppointmentTime,
        };

        var (ok, error) = await _apiClient.CreateAppointmentAsync(appointment);

        if (!ok)
        {
            TempData["ErrorMessage"] = $"Failed to create a new appointment. {error}";
            return RedirectToAction("Index", "Appointments");
        }

        TempData["SuccessMessage"] = "Appointment is created successfully.";
        return RedirectToAction("Index", "Appointments");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditAppointmentViewModel model)
    {
        if (!ModelState.IsValid) 
        {
            TempData["ErrorMessage"] = "Please fill in all required fields.";
            return RedirectToAction("Index", "Appointments");
        }

        var appointment = new UpdateAppointmentRequest
        {
            TherapistName = model.TherapistName,
            Title = model.Title,
            Notes = model.Notes,
            AppointmentTime = model.AppointmentTime,
            Status = model.Status,
        };

        var (ok, error) = await _apiClient.UpdateAppointmentAsync(model.AppointmentId, appointment);
        if (!ok)
        {
            TempData["ErrorMessage"] = $"Failed to update the appointment. {error}";
            return RedirectToAction("Index", "Appointments");
        }

        TempData["SuccessMessage"] = "Appointment is updated successfully.";
        return RedirectToAction("Index", "Appointments");
    }

    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _apiClient.GetAppointmentByIdAsync(id);
        if (appointment == null) return Json(new { error = "Not found" });
        return Json(appointment);
    }

    [HttpPost] 
    public async Task<IActionResult> CancelAppointment(Guid id)
    {
        var request = await _apiClient.CancelAppointmentAsync(id);

        if(!request)
        {
            TempData["ErrorMessage"] = "Failed to cancel this appointment.";
            return RedirectToAction("Index", "Appointments");
        }

        TempData["SuccessMessage"] = "Appointment cancelled successfully.";
        return RedirectToAction("Index", "Appointments");
    }
}
