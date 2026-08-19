using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class PatientAttachmentConfiguration : IEntityTypeConfiguration<PatientAttachment>
    {
        public void Configure(EntityTypeBuilder<PatientAttachment> builder)
        {
            builder.ToTable("PatientAttachments");
            builder.HasKey(a => a.PatientAttachmentId);

            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(260);

            builder.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.Content)
                .IsRequired();

            builder.Property(a => a.UploadedByEmail)
                .HasMaxLength(200);

            // Delete a patient → their attachments go too (matches the cascade from
            // ApplicationUser → Patient). Soft-delete is the normal path; this only
            // bites on a genuine hard delete.
            builder.HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Same clinic FK shape as Patient — Restrict so a clinic can't be removed
            // out from under rows that still point at it.
            builder.HasOne<Clinic>()
                .WithMany()
                .HasForeignKey(a => a.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.PatientId);
        }
    }
}
