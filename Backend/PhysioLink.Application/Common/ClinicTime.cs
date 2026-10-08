namespace PhysioLink.Application.Common
{
    // Appointment and slot times are stored as true UTC instants. Anything that
    // means a *clinic* wall-clock concept — operating hours, "today", the slot
    // grid's rows, a time shown in a push message — goes through this one zone.
    // Until clinics can set their own timezone, every clinic is Palestine (the
    // admin panel's Helpers/ClinicTime uses the same zone; keep them in sync).
    public static class ClinicTime
    {
        // IANA id: resolves on Linux (Railway) and on Windows with .NET's ICU tz data.
        private static readonly TimeZoneInfo Zone =
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Hebron");

        public static DateTime ToLocal(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

        // A wall-clock time that falls in the spring-forward gap doesn't exist;
        // push it past the gap rather than throwing.
        public static DateTime ToUtc(DateTime clinicLocal)
        {
            var local = DateTime.SpecifyKind(clinicLocal, DateTimeKind.Unspecified);
            if (Zone.IsInvalidTime(local))
                local = local.AddHours(1);
            return TimeZoneInfo.ConvertTimeToUtc(local, Zone);
        }

        public static DateTime Today => ToLocal(DateTime.UtcNow).Date;

        // UTC instant at which the clinic's current day began.
        public static DateTime TodayStartUtc => ToUtc(Today);
    }
}
