using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUserEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A filtered unique index on Users.Email (active rows only) is about to be
            // created. Existing data may already contain more than one *active* user
            // per email (e.g. a patient created before this guard existed, or a row
            // whose deactivation never marked the user IsDeleted). Dedupe first so the
            // index can be built: keep the newest active user per email, soft-delete
            // the rest, and soft-delete their patient rows to stay consistent.
            migrationBuilder.Sql(@"
                WITH victims AS (
                    SELECT ""ApplicationUserId""
                    FROM (
                        SELECT ""ApplicationUserId"",
                               ROW_NUMBER() OVER (
                                   PARTITION BY ""Email""
                                   ORDER BY ""CreatedAt"" DESC, ""ApplicationUserId"" DESC
                               ) AS rn
                        FROM ""Users""
                        WHERE ""IsDeleted"" = false
                    ) s
                    WHERE s.rn > 1
                )
                UPDATE ""Patients"" p
                SET ""IsDeleted"" = true, ""UpdatedAt"" = now()
                FROM victims v
                WHERE p.""ApplicationUserId"" = v.""ApplicationUserId""
                  AND p.""IsDeleted"" = false;");

            migrationBuilder.Sql(@"
                WITH victims AS (
                    SELECT ""ApplicationUserId""
                    FROM (
                        SELECT ""ApplicationUserId"",
                               ROW_NUMBER() OVER (
                                   PARTITION BY ""Email""
                                   ORDER BY ""CreatedAt"" DESC, ""ApplicationUserId"" DESC
                               ) AS rn
                        FROM ""Users""
                        WHERE ""IsDeleted"" = false
                    ) s
                    WHERE s.rn > 1
                )
                UPDATE ""Users"" u
                SET ""IsDeleted"" = true, ""UpdatedAt"" = now()
                FROM victims v
                WHERE u.""ApplicationUserId"" = v.""ApplicationUserId"";");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");
        }
    }
}
