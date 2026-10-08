namespace PhysioLink.AdminPanel.Helpers;

// Every appointment/slot time from the API is a true UTC instant. The panel shows
// and accepts clinic wall-clock time: convert with ToLocal for display and ToUtc
// for anything the admin types or picks before it goes to the API. Until clinics
// can set their own timezone this is one fixed zone — Palestine — and it must
// match the API's PhysioLink.Application.Common.ClinicTime.
public static class ClinicTime
{
    // IANA id — verified to resolve in this .NET runtime, unlike the Windows-style
    // "Jerusalem Standard Time" id, which threw TimeZoneNotFoundException here.
    private static readonly TimeZoneInfo Zone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Hebron");

    public static DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

    // A wall-clock time inside the spring-forward gap doesn't exist; push it past
    // the gap rather than throwing.
    public static DateTime ToUtc(DateTime clinicLocal)
    {
        var local = DateTime.SpecifyKind(clinicLocal, DateTimeKind.Unspecified);
        if (Zone.IsInvalidTime(local))
            local = local.AddHours(1);
        return TimeZoneInfo.ConvertTimeToUtc(local, Zone);
    }

    public static DateTime Today => ToLocal(DateTime.UtcNow).Date;
}
