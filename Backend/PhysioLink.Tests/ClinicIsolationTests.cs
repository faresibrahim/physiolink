using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.Interfaces;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Tests;

public class ClinicIsolationTests
{
    private static readonly Guid Clinic1Id = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Clinic2Id = new Guid("22222222-2222-2222-2222-222222222222");

    private const string ConnectionString =
        "Host=localhost;Port=5433;Database=PhysioLinkDb;Username=postgres;Password=Ibra@2026";

    private PhysioLinkDbContext CreateContext(Guid clinicId)
    {
        var options = new DbContextOptionsBuilder<PhysioLinkDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;
        return new PhysioLinkDbContext(options, new FakeClinicService(clinicId));
    }

    [Fact]
    public async Task Clinic1_Query_Returns_Only_Clinic1_Patients()
    {
        using var context = CreateContext(Clinic1Id);

        var patients = await context.Patients.ToListAsync();

        Assert.NotEmpty(patients);
        Assert.All(patients, p => Assert.Equal(Clinic1Id, p.ClinicId));
        Assert.DoesNotContain(patients, p => p.ClinicId == Clinic2Id);
    }

    [Fact]
    public async Task Clinic2_Query_Returns_Only_Clinic2_Patients()
    {
        using var context = CreateContext(Clinic2Id);

        var patients = await context.Patients.ToListAsync();

        Assert.NotEmpty(patients);
        Assert.All(patients, p => Assert.Equal(Clinic2Id, p.ClinicId));
        Assert.DoesNotContain(patients, p => p.ClinicId == Clinic1Id);
    }
}

internal class FakeClinicService : ICurrentClinicService
{
    private readonly Guid _clinicId;

    public FakeClinicService(Guid clinicId) => _clinicId = clinicId;

    public Guid? GetCurrentClinicId() => _clinicId;
}
