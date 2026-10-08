using PhysioLink.Application.Common;

namespace PhysioLink.Tests;

// Stored appointment/slot times are true UTC; ClinicTime is the single place that
// maps them to and from the clinic's (Palestine) wall clock.
public class ClinicTimeTests
{
    private static readonly TimeZoneInfo Palestine = TimeZoneInfo.FindSystemTimeZoneById("Asia/Hebron");

    [Theory]
    [InlineData("2026-08-25T10:00:00", "2026-08-25T07:00:00")] // summer, UTC+3
    [InlineData("2026-01-15T10:00:00", "2026-01-15T08:00:00")] // winter, UTC+2
    public void ToUtc_Uses_The_Offset_In_Effect_On_That_Date(string local, string expectedUtc)
    {
        var utc = ClinicTime.ToUtc(DateTime.Parse(local));

        Assert.Equal(DateTime.Parse(expectedUtc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(DateTime.Parse(local), ClinicTime.ToLocal(utc));
    }

    [Fact]
    public void ToUtc_Does_Not_Throw_For_A_Wall_Clock_Time_Skipped_By_Spring_Forward()
    {
        var gap = Enumerable.Range(0, 366 * 24)
            .Select(h => new DateTime(2026, 1, 1).AddHours(h))
            .First(Palestine.IsInvalidTime);

        var utc = ClinicTime.ToUtc(gap);

        Assert.Equal(gap.AddHours(1), ClinicTime.ToLocal(utc));
    }

    [Fact]
    public void TodayStartUtc_Is_Clinic_Midnight()
    {
        var local = ClinicTime.ToLocal(ClinicTime.TodayStartUtc);

        Assert.Equal(TimeSpan.Zero, local.TimeOfDay);
        Assert.Equal(ClinicTime.Today, local.Date);
    }
}
