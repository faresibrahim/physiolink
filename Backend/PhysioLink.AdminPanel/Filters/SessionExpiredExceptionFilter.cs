using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PhysioLink.AdminPanel.Services;

namespace PhysioLink.AdminPanel.Filters;

// Turns a SessionExpiredException (raised deep inside ApiClient when a refresh
// finally fails) into a clean sign-out + redirect to the login page. Without this
// the panel would swallow the null result and render a dead page full of zeros.
public class SessionExpiredExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not SessionExpiredException) return;

        var http = context.HttpContext;

        // Drop every trace of the dead session so the next request starts clean.
        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        http.Response.Cookies.Delete("auth_token");
        http.Response.Cookies.Delete("refresh_token");

        context.ExceptionHandled = true;

        // AJAX/fetch callers can't follow a 302 into an HTML login page — answer
        // with 401 so their JS can redirect the top-level window itself.
        if (IsAjaxRequest(http.Request))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            return;
        }

        var tempFactory = http.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
        var tempData = tempFactory.GetTempData(http);
        tempData["ErrorMessage"] = "Your session has expired. Please log in again.";

        context.Result = new RedirectToActionResult("Login", "Auth", null);
    }

    private static bool IsAjaxRequest(HttpRequest request)
    {
        if (request.Headers["X-Requested-With"] == "XMLHttpRequest") return true;

        var accept = request.Headers.Accept.ToString();
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
