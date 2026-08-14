using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
    {
        public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
        {
            builder.ToTable("AppointmentSlots");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.ScheduledAt)
                .IsRequired();

            builder.Property(s => s.Status)
                .IsRequired();

            // D1 — every slot has a therapist. Restrict stops a therapist with
            // future slots from being hard-deleted (we soft-delete anyway).
            builder.HasOne(s => s.Therapist)
                .WithMany()
                .HasForeignKey(s => s.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);

            // Clinic FK — tenant scope. Restrict to mirror the tenant-safety posture.
            builder.HasOne<Clinic>()
                .WithMany()
                .HasForeignKey(s => s.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevents two live slots for the same therapist at the same hour — the
            // toggle must never create a duplicate. Filtered so soft-deleted rows
            // don't block re-creating a slot at the same time.
            builder.HasIndex(s => new { s.TherapistId, s.ScheduledAt })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            // The patient slot-list query filters by therapist + status + time.
            builder.HasIndex(s => new { s.TherapistId, s.Status, s.ScheduledAt });

            // Consume-on-request race safety is handled by the conditional
            // ExecuteUpdateAsync guard in Phase 3.2 (only flips a slot that is still
            // Available), so no xmin/rowversion concurrency token is configured here.
            // Spec 1.7 explicitly allows either mechanism, not both.
        }
    }
}
