namespace PhysioLink.Tests;

/// <summary>
/// Connection string for the integration tests that hit the local dev Postgres
/// (port 5433). The password comes from the TEST_DB_PASSWORD environment variable
/// so it is never committed; see .env.example at the repo root.
/// </summary>
internal static class TestDb
{
    public const string PasswordVariable = "TEST_DB_PASSWORD";

    public static string ConnectionString
    {
        get
        {
            var password = Environment.GetEnvironmentVariable(PasswordVariable);
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(
                    $"{PasswordVariable} is not set. Set it to the local dev Postgres password " +
                    $"(port 5433) before running the DB tests, e.g. in PowerShell: " +
                    $"$env:{PasswordVariable}=\"<password>\"; dotnet test");
            }

            return $"Host=localhost;Port=5433;Database=PhysioLinkDb;Username=postgres;Password={password}";
        }
    }
}
