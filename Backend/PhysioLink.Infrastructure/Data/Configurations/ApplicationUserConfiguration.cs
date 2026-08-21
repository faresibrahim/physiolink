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
            .HasMaxLength(200);

            // One active user per email. Filtered so soft-deleted rows don't block
            // re-creating a patient with the same email after deactivation. Multiple
            // NULLs (ClinicAdmin rows and any emailless patients) don't collide under
            // a unique index.
            builder.HasIndex(p => p.Email)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            builder.Property(p => p.Username)
            .HasMaxLength(50);

            // One active user per username. Only Patient rows ever get a Username —
            // ClinicAdmin rows stay null and don't collide (see above).
            builder.HasIndex(p => p.Username)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            builder.Property(p=>p.PasswordHash)
            .IsRequired();

            builder.Property(u => u.Role)
                .HasDefaultValue("Patient")
                .IsRequired();

            builder.Property(u => u.ClinicId)
                .IsRequired();

            builder.Property(u => u.MustChangePassword)
                .HasDefaultValue(false);

            builder.HasOne<Clinic>()
    .WithMany()
    .HasForeignKey(u => u.ClinicId)
    .OnDelete(DeleteBehavior.Restrict);

        }
    }
}