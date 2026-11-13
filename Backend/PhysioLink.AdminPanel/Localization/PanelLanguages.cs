using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace PhysioLink.AdminPanel.Localization;

// The two languages the panel ships in, and the cookie that remembers the choice.
public static class PanelLanguages
{
    public const string English = "en-US";
    public const string Arabic = "ar-PS";

    public static readonly string CookieName = CookieRequestCultureProvider.DefaultCookieName;

    public static readonly CultureInfo EnglishCulture = CultureInfo.ReadOnly(new CultureInfo(English));

    // Palestinian Arabic for month/day names (matches the clinic's timezone), but:
    //  · always the Gregorian calendar, never a Hijri one;
    //  · invariant number formatting, so "1.5" stays "1.5" and anything echoed into
    //    a form field or parsed back from one is unaffected by the UI language.
    public static readonly CultureInfo ArabicCulture = CreateArabic();

    public static IList<CultureInfo> Supported => new[] { EnglishCulture, ArabicCulture };

    public static bool IsSupported(string? culture) =>
        culture == English || culture == Arabic;

    public static bool IsArabic => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

    // Writes the language cookie: one year, first-party, survives sign-out.
    public static void Remember(HttpResponse response, string culture, bool secure)
    {
        response.Cookies.Append(
            CookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = secure,
                Path = "/"
            });
    }

    private static CultureInfo CreateArabic()
    {
        var culture = new CultureInfo(Arabic);
        if (culture.DateTimeFormat.Calendar is not GregorianCalendar)
        {
            var gregorian = culture.OptionalCalendars.OfType<GregorianCalendar>().FirstOrDefault();
            if (gregorian != null) culture.DateTimeFormat.Calendar = gregorian;
        }
        culture.NumberFormat = CultureInfo.InvariantCulture.NumberFormat;
        return CultureInfo.ReadOnly(culture);
    }
}
