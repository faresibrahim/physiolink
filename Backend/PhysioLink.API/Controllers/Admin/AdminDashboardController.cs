using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers.Admin
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1/admin/dashboard")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _adminDashboardService.GetAsync();
            return Ok(result);
        }
    }
}
