namespace PhysioLink.AdminPanel.Localization;

// Display date patterns in the order each language reads them: English puts the
// month first ("Nov 9, 2026"), Arabic the day ("9 تشرين الثاني 2026"). Month
// and weekday names come from the request culture either way. Machine formats
// (yyyy-MM-dd for links and form values) are not here and never change.
public static class DatePatterns
{
    public static string MonthDay => PanelLanguages.IsArabic ? "d MMM" : "MMM d";
    public static string MonthDayYear => PanelLanguages.IsArabic ? "d MMM yyyy" : "MMM d, yyyy";
    public static string MonthDayTime => PanelLanguages.IsArabic ? "d MMM، HH:mm" : "MMM d, HH:mm";
    public static string WeekdayMonthDay => PanelLanguages.IsArabic ? "dddd، d MMM" : "dddd, MMM d";
    public static string ShortWeekdayMonthDay => PanelLanguages.IsArabic ? "dddd، d MMM" : "ddd, MMM d";
}
