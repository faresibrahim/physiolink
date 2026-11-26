using System.Globalization;

namespace PhysioLink.Application.Common
{
    // The wire format for the daily exercise reminder time: 24-hour "HH:mm" in
    // CLINIC local time (ClinicTime's zone). No zone is ever sent or stored.
    public static class ReminderTimeFormat
    {
        public const string Pattern = "HH:mm";

        // Strict: "07:30" ok; "7:30", "24:00", "25:99", "18:00:00" rejected.
        public static bool TryParse(string? value, out TimeOnly time) =>
            TimeOnly.TryParseExact(value, Pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out time);

        public static string Format(TimeOnly time) =>
            time.ToString(Pattern, CultureInfo.InvariantCulture);
    }
}
