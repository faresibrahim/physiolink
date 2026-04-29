using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.ToTable("Clinics");
            builder.HasKey(c => c.ClinicId);



            builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);
            builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

            builder.Property(c=>c.IsActive)
                .IsRequired();


        }
    }
}