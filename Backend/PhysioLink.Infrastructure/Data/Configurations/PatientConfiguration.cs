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
            .HasMaxLength(200);

            builder.Property(p => p.Username)
            .IsRequired()
            .HasMaxLength(50);

            builder.Property(p=>p.PhoneNumber)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(p=>p.Diagnosis) 
            .IsRequired()
            .HasMaxLength(500);

            builder.HasOne(p => p.Therapist)
                .WithMany()
                .HasForeignKey(p=>p.TherapistId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.Clinic)
                .WithMany()
                .HasForeignKey(p=>p.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Patient>(p=>p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

            // Postgres "time" / "date" — clinic-local wall-clock values, no zone.
            // No HasDefaultValue on purpose: EF would treat a CLR 00:00 as "unset" and
            // let the DB default overwrite a deliberate midnight. Existing rows get
            // 18:00 from the migration's column default instead.
            builder.Property(p => p.ReminderTime)
            .HasColumnType("time without time zone");

            builder.Property(p => p.LastReminderSentOn)
            .HasColumnType("date");

            // The reminder scheduler scans "enabled and due by now" every minute.
            // Partial index: only the (few) opted-in patients are indexed.
            builder.HasIndex(p => p.ReminderTime)
            .HasDatabaseName("IX_Patients_ReminderTime_Enabled")
            .HasFilter("\"ReminderEnabled\" = true");

        }
    }
}