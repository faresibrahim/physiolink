using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class TherapistConfiguration : IEntityTypeConfiguration<Therapist>
    {
        public void Configure(EntityTypeBuilder<Therapist> builder)
        {
            builder.ToTable("Therapists");
            builder.HasKey(t => t.TherapistId);

          

            builder.Property(t => t.FirstName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(t => t.LastName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(t => t.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

            builder.Property(t => t.Speciality)
            .IsRequired()
            .HasMaxLength(500);

            builder.HasOne<Clinic>()
    .WithMany()
    .HasForeignKey(t => t.ClinicId)
    .OnDelete(DeleteBehavior.Cascade);

        }
    }
}