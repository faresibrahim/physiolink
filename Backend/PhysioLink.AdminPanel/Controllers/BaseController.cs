using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace PhysioLink.AdminPanel.Controllers;

public abstract class BaseController : Controller
{
    // For messages that carry a value (a file name, an API error). Fixed messages
    // stay as English keys and are translated where the layout shows them.
    protected IStringLocalizer L =>
        HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

    protected IActionResult SessionExpiredRedirect()
    {
        TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
        return RedirectToAction("Login", "Auth");
    }
}
