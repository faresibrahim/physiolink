using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Localization;

namespace PhysioLink.AdminPanel.Controllers;

// First-visit language screen, plus the switch used by the sidebar and login page.
[AllowAnonymous]
public class LanguageController : Controller
{
    // GET /Language — shown once per browser (FirstVisitLanguageMiddleware).
    [HttpGet]
    public IActionResult Index(string? returnUrl)
    {
        ViewData["ReturnUrl"] = SafeReturnUrl(returnUrl);
        return View();
    }

    // POST /Language/Set — remembers the choice and moves on. Signed-out visitors
    // land on the login page (which plays its entrance when fromPicker is set);
    // signed-in ones (the sidebar switch) go back to the page they were on.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string? returnUrl, bool fromPicker = false)
    {
        if (!PanelLanguages.IsSupported(culture))
            culture = PanelLanguages.English;

        PanelLanguages.Remember(Response, culture, Request.IsHttps);

        var target = SafeReturnUrl(returnUrl);
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(target);

        if (fromPicker)
            TempData["FromLanguagePicker"] = true;

        return RedirectToAction("Login", "Auth");
    }

    private string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}
