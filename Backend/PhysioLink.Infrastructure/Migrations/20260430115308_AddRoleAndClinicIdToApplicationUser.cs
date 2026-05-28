using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndClinicIdToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "Patient");

            migrationBuilder.Sql(@"
                UPDATE ""Users"" SET ""ClinicId"" = '11111111-1111-1111-1111-111111111111'
                WHERE ""ClinicId"" = '00000000-0000-0000-0000-000000000000'
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClinicId",
                table: "Users",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Clinics_ClinicId",
                table: "Users",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "ClinicId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Clinics_ClinicId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClinicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
