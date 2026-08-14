using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CloseHour",
                table: "Clinics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpenHour",
                table: "Clinics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Seed operating hours on existing clinics (spec 1.5 / 1.8 step 3). The
            // columns default to 0/0 which is an empty window; give every clinic a
            // usable 8:00–18:00 grid.
            migrationBuilder.Sql(
                "UPDATE \"Clinics\" SET \"OpenHour\" = 8, \"CloseHour\" = 18 WHERE \"CloseHour\" <= \"OpenHour\";");

            // Remap any legacy AppointmentStatus rows to the expanded enum (spec 1.2 /
            // 1.8 step 4). Old set: Pending=0, Confirmed=1, Cancelled=2, Completed=3.
            // New set: Requested=0, Confirmed=1, Completed=2, Rejected=3, Expired=4,
            // CancelledByClinic=5. 0/1 keep their meaning. Move 2 -> 5 BEFORE 3 -> 2
            // so the two updates never collide.
            migrationBuilder.Sql(
                "UPDATE \"Appointments\" SET \"Status\" = 5 WHERE \"Status\" = 2;");
            migrationBuilder.Sql(
                "UPDATE \"Appointments\" SET \"Status\" = 2 WHERE \"Status\" = 3;");

            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentSlotId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TherapistId",
                table: "Appointments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AppointmentSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TherapistId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentSlots_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "ClinicId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppointmentSlots_Therapists_TherapistId",
                        column: x => x.TherapistId,
                        principalTable: "Therapists",
                        principalColumn: "TherapistId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentSlotId",
                table: "Appointments",
                column: "AppointmentSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TherapistId",
                table: "Appointments",
                column: "TherapistId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_ClinicId",
                table: "AppointmentSlots",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_TherapistId_ScheduledAt",
                table: "AppointmentSlots",
                columns: new[] { "TherapistId", "ScheduledAt" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_TherapistId_Status_ScheduledAt",
                table: "AppointmentSlots",
                columns: new[] { "TherapistId", "Status", "ScheduledAt" });

            // Appointments.TherapistId is a new non-null FK (spec 1.4). Existing rows
            // were added with the empty-Guid default above, which would fail the FK
            // below. Backfill each legacy appointment to the first live therapist in
            // its own clinic before the constraint is created (spec 1.8). Seed data
            // guarantees every clinic has therapists; a clinic with none would leave
            // its legacy rows unresolved.
            migrationBuilder.Sql(@"
                UPDATE ""Appointments"" a
                SET ""TherapistId"" = (
                    SELECT t.""TherapistId""
                    FROM ""Therapists"" t
                    WHERE t.""ClinicId"" = a.""ClinicId"" AND t.""IsDeleted"" = false
                    ORDER BY t.""CreatedAt""
                    LIMIT 1
                )
                WHERE a.""TherapistId"" = '00000000-0000-0000-0000-000000000000'
                  AND EXISTS (
                    SELECT 1 FROM ""Therapists"" t2
                    WHERE t2.""ClinicId"" = a.""ClinicId"" AND t2.""IsDeleted"" = false
                  );");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AppointmentSlots_AppointmentSlotId",
                table: "Appointments",
                column: "AppointmentSlotId",
                principalTable: "AppointmentSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Therapists_TherapistId",
                table: "Appointments",
                column: "TherapistId",
                principalTable: "Therapists",
                principalColumn: "TherapistId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AppointmentSlots_AppointmentSlotId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Therapists_TherapistId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "AppointmentSlots");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AppointmentSlotId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TherapistId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CloseHour",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "OpenHour",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "AppointmentSlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TherapistId",
                table: "Appointments");
        }
    }
}
