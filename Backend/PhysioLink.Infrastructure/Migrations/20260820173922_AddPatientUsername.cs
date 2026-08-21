using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysioLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Patients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Patients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Backfill: every pre-existing patient logged in with Email and has no
            // Username yet. Derive one from their name (first.last, lower-cased,
            // stripped to [a-z0-9.]) and dedupe with a numeric suffix on collision,
            // so nobody's login breaks. Mirrors the existing patient's Email up to
            // this point — Username becomes the new login identifier going forward.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    rec RECORD;
                    base_username TEXT;
                    candidate TEXT;
                    suffix INT;
                BEGIN
                    FOR rec IN
                        SELECT p.""PatientId"", p.""ApplicationUserId"", p.""FirstName"", p.""LastName""
                        FROM ""Patients"" p
                        ORDER BY p.""PatientId""
                    LOOP
                        base_username := regexp_replace(lower(rec.""FirstName"" || '.' || rec.""LastName""), '[^a-z0-9.]', '', 'g');
                        IF base_username = '' THEN
                            base_username := 'patient';
                        END IF;

                        candidate := base_username;
                        suffix := 1;
                        WHILE EXISTS (SELECT 1 FROM ""Users"" WHERE ""Username"" = candidate) LOOP
                            suffix := suffix + 1;
                            candidate := base_username || '.' || suffix::text;
                        END LOOP;

                        UPDATE ""Users"" SET ""Username"" = candidate WHERE ""ApplicationUserId"" = rec.""ApplicationUserId"";
                        UPDATE ""Patients"" SET ""Username"" = candidate WHERE ""PatientId"" = rec.""PatientId"";
                    END LOOP;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Patients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
