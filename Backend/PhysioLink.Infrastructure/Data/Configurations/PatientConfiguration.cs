using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(p=>p.PatientId);

            builder.Property(p=>p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(p=>p.LastName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(p=>p.Email)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(p=>p.PhoneNumber)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(p=>p.Diagnosis) 
            .IsRequired()
            .HasMaxLength(500);

            builder.HasOne<Therapist>()
                .WithMany()
                .HasForeignKey(p=>p.TherapistId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<Clinic>()
                .WithMany()
                .HasForeignKey(p=>p.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Patient>(p=>p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}