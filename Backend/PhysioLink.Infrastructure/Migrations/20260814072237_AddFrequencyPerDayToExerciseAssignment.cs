using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFrequencyPerDayToExerciseAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FrequencyPerDay",
                table: "ExerciseAssignments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrequencyPerDay",
                table: "ExerciseAssignments");
        }
    }
}
