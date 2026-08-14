using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;

namespace PhysioLink.AdminPanel.ViewComponents;

// Renders the sidebar "Requests" nav item together with a live count of pending
// appointment requests. Invoked from _Layout, so it runs on every authenticated
// page; a failed/empty fetch degrades to a count of 0 (no badge).
public class RequestsNavViewComponent : ViewComponent
{
    private readonly ApiClient _apiClient;

    public RequestsNavViewComponent(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var requests = await _apiClient.GetAppointmentRequestsAsync(null);
        return View(requests?.Count ?? 0);
    }
}
