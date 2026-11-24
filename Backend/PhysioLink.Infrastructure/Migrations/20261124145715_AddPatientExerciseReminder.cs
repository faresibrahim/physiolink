using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientExerciseReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastReminderSentOn",
                table: "Patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderEnabled",
                table: "Patients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ReminderTime",
                table: "Patients",
                type: "time without time zone",
                nullable: false,
                // Existing patients start with the app's default reminder time
                // (Patient.DefaultReminderTime), disabled. The model has no default
                // on purpose — see PatientConfiguration.
                defaultValue: new TimeOnly(18, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ReminderTime_Enabled",
                table: "Patients",
                column: "ReminderTime",
                filter: "\"ReminderEnabled\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_ReminderTime_Enabled",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LastReminderSentOn",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReminderEnabled",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReminderTime",
                table: "Patients");
        }
    }
}
