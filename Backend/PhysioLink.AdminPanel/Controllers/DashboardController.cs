using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels;

namespace PhysioLink.AdminPanel.Controllers
{
    //Authorize is added on top of the class
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly ApiClient _apiClient;
        public DashboardController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            var viewModel = new DashboardViewModel();

            var dashboard          = await _apiClient.GetDashboardStatsAsync();
            var appointments       = await _apiClient.GetUpcomingAppointmentsAsync(5);
            var recentPatients     = await _apiClient.GetRecentPatientsAsync(5);

            if (dashboard != null)
            {
                viewModel.TherapistCount       = dashboard.TherapistCount;
                viewModel.PatientCount         = dashboard.PatientCount;
                viewModel.ThisWeekAppointments = dashboard.AppointmentsThisWeek;
                viewModel.ActiveAssignment     = dashboard.ActiveAssignmentCount;
            }

            viewModel.UpcomingAppointments = appointments?.Items ?? [];
            viewModel.RecentPatients       = recentPatients?.Items ?? [];

            return View(viewModel);
        }
    }
}
