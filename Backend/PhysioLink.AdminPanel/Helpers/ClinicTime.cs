namespace PhysioLink.AdminPanel.Helpers;

// Every appointment/slot time from the API is UTC. Until clinics can set their
// own timezone, every admin-panel display converts through this single fixed
// zone — Palestine (matches the +970 numbers in the seeded dev data). Swap the
// zone ID here if the deployed clinic is actually elsewhere.
//
// Scope note: this only affects *read-only display* formatting (list rows,
// dashboard). The appointment edit modal's datetime-local field is left as a
// raw UTC round-trip — converting that too would mean converting both the
// prefill AND the submitted value, and a mistake there could actually shift a
// real appointment's stored time.
public static class ClinicTime
{
    // IANA id — verified to resolve in this .NET runtime, unlike the Windows-style
    // "Jerusalem Standard Time" id, which threw TimeZoneNotFoundException here.
    private static readonly TimeZoneInfo Zone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Hebron");

    public static DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);
}
