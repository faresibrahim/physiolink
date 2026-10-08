using Microsoft.EntityFrameworkCore;
using PhysioLink.Infrastructure.Data;
using PhysioLink.Infrastructure.Repositories;

namespace PhysioLink.Tests;

// An FCM token identifies a phone, not a patient. When a second patient logs in on
// the same phone, the token must move to them and leave the first patient's record —
// otherwise the first patient's appointment/exercise pushes land on the new user's
// phone.
public class DeviceTokenTests
{
    private static readonly Guid Clinic1Id = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Clinic2Id = new Guid("22222222-2222-2222-2222-222222222222");

    private const string ConnectionString =
        "Host=localhost;Port=5433;Database=PhysioLinkDb;Username=postgres;Password=Ibra@2026";

    private static PhysioLinkDbContext CreateContext(Guid clinicId)
    {
        var options = new DbContextOptionsBuilder<PhysioLinkDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;
        return new PhysioLinkDbContext(options, new FakeClinicService(clinicId));
    }

    [Fact]
    public async Task Registering_A_Token_Removes_It_From_The_Previous_Patient_Across_Clinics()
    {
        Guid firstId, secondId;
        string? firstOriginal, secondOriginal;
        using (var ctx = CreateContext(Clinic1Id))
        {
            var p = await ctx.Patients.IgnoreQueryFilters().FirstAsync(x => x.ClinicId == Clinic1Id);
            (firstId, firstOriginal) = (p.PatientId, p.DeviceToken);
        }
        using (var ctx = CreateContext(Clinic2Id))
        {
            var p = await ctx.Patients.IgnoreQueryFilters().FirstAsync(x => x.ClinicId == Clinic2Id);
            (secondId, secondOriginal) = (p.PatientId, p.DeviceToken);
        }

        var sharedToken = $"test-token-{Guid.NewGuid()}";
        try
        {
            using (var ctx = CreateContext(Clinic1Id))
                Assert.True(await new PatientRepository(ctx).UpdateDeviceTokenAsync(firstId, sharedToken));

            using (var ctx = CreateContext(Clinic2Id))
                Assert.True(await new PatientRepository(ctx).UpdateDeviceTokenAsync(secondId, sharedToken));

            using var verify = CreateContext(Clinic1Id);
            var holders = await verify.Patients.IgnoreQueryFilters()
                .Where(p => p.DeviceToken == sharedToken)
                .Select(p => p.PatientId)
                .ToListAsync();

            Assert.Equal(new[] { secondId }, holders);
        }
        finally
        {
            using var restore = CreateContext(Clinic1Id);
            var first = await restore.Patients.IgnoreQueryFilters().FirstAsync(p => p.PatientId == firstId);
            var second = await restore.Patients.IgnoreQueryFilters().FirstAsync(p => p.PatientId == secondId);
            first.DeviceToken = firstOriginal;
            second.DeviceToken = secondOriginal;
            await restore.SaveChangesAsync();
        }
    }
}
