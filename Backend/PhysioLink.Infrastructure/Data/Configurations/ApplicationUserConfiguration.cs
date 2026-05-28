using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(p=>p.ApplicationUserId);

            builder.Property(p=>p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(p=>p.LastName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(p=>p.Email)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(p=>p.PasswordHash)
            .IsRequired();

            builder.Property(u => u.Role)
                .HasDefaultValue("Patient")
                .IsRequired();

            builder.Property(u => u.ClinicId)
                .IsRequired();

            builder.HasOne<Clinic>()
    .WithMany()
    .HasForeignKey(u => u.ClinicId)
    .OnDelete(DeleteBehavior.Restrict);

        }
    }
}