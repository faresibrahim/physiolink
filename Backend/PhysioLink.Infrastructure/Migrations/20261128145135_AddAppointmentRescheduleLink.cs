using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentRescheduleLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RescheduleOfAppointmentId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_RescheduleOfAppointmentId",
                table: "Appointments",
                column: "RescheduleOfAppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Appointments_RescheduleOfAppointmentId",
                table: "Appointments",
                column: "RescheduleOfAppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Appointments_RescheduleOfAppointmentId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_RescheduleOfAppointmentId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RescheduleOfAppointmentId",
                table: "Appointments");
        }
    }
}
