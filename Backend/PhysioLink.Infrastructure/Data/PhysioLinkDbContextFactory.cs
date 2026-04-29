using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Infrastructure.Data
{
    public class PhysioLinkDbContextFactory : IDesignTimeDbContextFactory<PhysioLinkDbContext>
    {
        public PhysioLinkDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PhysioLinkDbContext>();
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"));

            return new PhysioLinkDbContext(optionsBuilder.Options, new NullCurrentClinicService());
        }
    }

    internal class NullCurrentClinicService : ICurrentClinicService
    {
        public Guid? GetCurrentClinicId() => null;
    }
}
