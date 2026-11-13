namespace PhysioLink.AdminPanel.Localization;

// A browser that has never picked a language is sent to the language screen
// once; after it chooses, the culture cookie exists and this never fires again.
// Only full-page GET navigations are redirected — fetch()/JSON calls, form posts,
// static files and the health probe pass straight through.
public class FirstVisitLanguageMiddleware
{
    private readonly RequestDelegate _next;

    public FirstVisitLanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        if (HttpMethods.IsGet(request.Method)
            && !request.Cookies.ContainsKey(PanelLanguages.CookieName)
            && !request.Path.StartsWithSegments("/Language")
            && !request.Path.StartsWithSegments("/health")
            && AcceptsHtml(request))
        {
            var returnUrl = request.PathBase + request.Path + request.QueryString;
            context.Response.Redirect("/Language?returnUrl=" + Uri.EscapeDataString(returnUrl));
            return Task.CompletedTask;
        }

        return _next(context);
    }

    private static bool AcceptsHtml(HttpRequest request) =>
        request.Headers.Accept.Any(v => v != null && v.Contains("text/html", StringComparison.OrdinalIgnoreCase));
}
