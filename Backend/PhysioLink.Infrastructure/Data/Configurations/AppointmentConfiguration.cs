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

            builder.Property(p => p.TherapistId)
                .IsRequired();

            builder.Property(p => p.AppointmentTime)
                .IsRequired();

            builder.Property(p => p.Status)
                .IsRequired();
        }
    }
}
