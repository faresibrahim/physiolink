using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(p => p.AppointmentId);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasDefaultValue("Appointment Request");

            builder.Property(p => p.Notes)
                .HasMaxLength(1000);

            builder.Property(p => p.PatientId)
                .IsRequired();

            builder.Property(p => p.TherapistName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.AppointmentTime)
                .IsRequired();

            builder.Property(p => p.Status)
                .IsRequired();

            builder.HasOne<Clinic>()
                .WithMany()
                .HasForeignKey(c => c.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Owner therapist — non-null going forward; Restrict so a therapist with
            // appointment history can't be hard-deleted (spec 1.4).
            builder.Property(p => p.TherapistId)
                .IsRequired();

            builder.HasOne<Therapist>()
                .WithMany()
                .HasForeignKey(p => p.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);

            // Loose, nullable link to the originating slot (spec 1.4 / D2).
            builder.HasOne(p => p.AppointmentSlot)
                .WithMany()
                .HasForeignKey(p => p.AppointmentSlotId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
