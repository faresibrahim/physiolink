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

        // Appointments drop off the board once their due date has passed — they live in
        // History from then on. Clamping the window's start to today enforces that even
        // when the user pages back to (part of) a week that has already begun; a fully
        // past week yields an empty board.
        var from = computedWeekStart < today ? today : computedWeekStart;
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
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
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

    // GET /Appointments/History?page=1&status=&therapistFilter=
    // The archive of finished appointments (completed, cancelled, rejected, expired),
    // i.e. everything whose due date has passed and left the board.
    [HttpGet]
    public async Task<IActionResult> History(int page = 1, string? status = null, string? therapistFilter = null)
    {
        var historyTask    = _apiClient.GetAppointmentHistoryAsync(page, 20, status: status, therapistName: therapistFilter);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);
        await Task.WhenAll(historyTask, therapistsTask);

        var history    = await historyTask;
        var therapists = await therapistsTask;

        if (history == null)
        {
            TempData["ErrorMessage"] = "Failed to load appointment history. Please try again.";
            return RedirectToAction("Index", "Appointments");
        }

        var filters = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(status))          filters["status"]          = status;
        if (!string.IsNullOrWhiteSpace(therapistFilter)) filters["therapistFilter"] = therapistFilter;

        var viewModel = new AppointmentHistoryViewModel
        {
            Appointments    = history.Items,
            TotalCount      = history.TotalCount,
            StatusFilter    = status,
            TherapistFilter = therapistFilter,
            Therapists      = therapists?.Items ?? [],
            Pagination = new PaginationViewModel
            {
                ActionName  = "History",
                TotalPages  = history.TotalPages,
                CurrentPage = page,
                Filters     = filters,
            },
        };

        return View(viewModel);
    }

    // GET /Appointments/Requests?therapistFilter=
    [HttpGet]
    public async Task<IActionResult> Requests(Guid? therapistFilter = null)
    {
        var requestsTask = _apiClient.GetAppointmentRequestsAsync(therapistFilter);
        var therapistsTask = _apiClient.GetTherapistsAsync(1, 100);
        await Task.WhenAll(requestsTask, therapistsTask);

        var requests = await requestsTask;
        var therapists = await therapistsTask;

        if (requests == null)
        {
            TempData["ErrorMessage"] = "Failed to load the requests queue. Please try again.";
            return RedirectToAction("Index", "Dashboard");
        }

        var viewModel = new AppointmentRequestListViewModel
        {
            Requests = requests,
            Therapists = therapists?.Items ?? [],
            TherapistFilter = therapistFilter
        };

        return View(viewModel);
    }

    // POST /Appointments/AcceptRequest
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptRequest(Guid id, Guid? therapistFilter)
    {
        var success = await _apiClient.AcceptAppointmentRequestAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Request accepted — the slot is now booked."
            : "Could not accept this request. It may have expired or already been decided.";

        return RedirectToAction(nameof(Requests), new { therapistFilter });
    }

    // POST /Appointments/RejectRequest
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(Guid id, Guid? therapistFilter)
    {
        var success = await _apiClient.RejectAppointmentRequestAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Request rejected — the slot is free again."
            : "Could not reject this request. It may have expired or already been decided.";

        return RedirectToAction(nameof(Requests), new { therapistFilter });
    }

    // POST /Appointments/Confirm — confirm a pending request from the week list.
    // Mirrors AcceptRequest but returns to the appointments week the action came from.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id, DateTime? weekStart, string? status, string? therapistFilter)
    {
        var success = await _apiClient.AcceptAppointmentRequestAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Appointment confirmed."
            : "Could not confirm this appointment. It may have expired or already been decided.";

        return RedirectToAction(nameof(Index), new { weekStart, status, therapistFilter });
    }

    // POST /Appointments/Reject — reject a pending request from the week list.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, DateTime? weekStart, string? status, string? therapistFilter)
    {
        var success = await _apiClient.RejectAppointmentRequestAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Request rejected — the slot is free again."
            : "Could not reject this appointment. It may have expired or already been decided.";

        return RedirectToAction(nameof(Index), new { weekStart, status, therapistFilter });
    }

    // POST /Appointments/CancelConfirmed — clinic cancels a confirmed booking (D8)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(Guid id, DateTime? weekStart, string? status, string? therapistFilter)
    {
        var success = await _apiClient.CancelConfirmedAppointmentAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Appointment cancelled — the slot is free again."
            : "Could not cancel this appointment. It may not be confirmed.";

        return RedirectToAction(nameof(Index), new { weekStart, status, therapistFilter });
    }
}
