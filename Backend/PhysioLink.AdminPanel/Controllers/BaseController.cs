using Microsoft.AspNetCore.Mvc;

namespace PhysioLink.AdminPanel.Controllers;

public abstract class BaseController : Controller
{
    protected IActionResult SessionExpiredRedirect()
    {
        TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
        return RedirectToAction("Login", "Auth");
    }
}
