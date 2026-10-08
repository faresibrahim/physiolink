using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertAppointmentTimesToTrueUtc : Migration
    {
        // Until now the slot grid and the patient app treated a stored slot/appointment
        // time's UTC wall clock as *clinic* time (a "10:00" slot was stored as 10:00Z),
        // while the admin lists converted it to Palestine time — so the same booking
        // showed different times to staff and patients, and expiry/"still bookable"
        // checks ran 2–3h late. The code now stores true UTC instants and converts at
        // the edges (Application/Common/ClinicTime). This re-reads every existing value
        // as Asia/Hebron wall-clock time and stores the real instant (DST-aware per row).
        //
        // CreatedAt/UpdatedAt were always written from DateTime.UtcNow and are untouched.
        private const string Zone = "Asia/Hebron";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The unique (TherapistId, ScheduledAt) index is checked row by row during
            // an UPDATE, so shifting every slot back by the same offset can collide
            // with a not-yet-shifted neighbour (11:00 -> 08:00 while the 08:00 row
            // still exists). Drop it for the shift and rebuild it afterwards.
            migrationBuilder.DropIndex(
                name: "IX_AppointmentSlots_TherapistId_ScheduledAt",
                table: "AppointmentSlots");

            migrationBuilder.Sql($@"
                UPDATE ""AppointmentSlots""
                SET ""ScheduledAt"" = (""ScheduledAt"" AT TIME ZONE 'UTC') AT TIME ZONE '{Zone}';

                UPDATE ""Appointments""
                SET ""AppointmentTime"" = (""AppointmentTime"" AT TIME ZONE 'UTC') AT TIME ZONE '{Zone}';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_TherapistId_ScheduledAt",
                table: "AppointmentSlots",
                columns: new[] { "TherapistId", "ScheduledAt" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppointmentSlots_TherapistId_ScheduledAt",
                table: "AppointmentSlots");

            migrationBuilder.Sql($@"
                UPDATE ""AppointmentSlots""
                SET ""ScheduledAt"" = (""ScheduledAt"" AT TIME ZONE '{Zone}') AT TIME ZONE 'UTC';

                UPDATE ""Appointments""
                SET ""AppointmentTime"" = (""AppointmentTime"" AT TIME ZONE '{Zone}') AT TIME ZONE 'UTC';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_TherapistId_ScheduledAt",
                table: "AppointmentSlots",
                columns: new[] { "TherapistId", "ScheduledAt" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
