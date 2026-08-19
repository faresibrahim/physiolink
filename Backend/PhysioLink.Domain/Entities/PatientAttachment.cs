namespace PhysioLink.Domain.Entities
{
    // A file (PDF, image, document, …) a therapist uploads against a patient's
    // record — e.g. a referral letter, scan, or consent form. Bytes are stored
    // in the row itself (see EF config: bytea) so they survive Railway redeploys,
    // whose container filesystem is ephemeral. Clinic-scoped + soft-deletable via
    // the base, so isolation and "data is retained on delete" come for free.
    public class PatientAttachment : ClinicScopedEntity
    {
        public Guid PatientAttachmentId { get; set; }
        public Guid PatientId { get; set; }
        public Patient? Patient { get; set; }

        // Original client filename, shown in the UI and used for downloads.
        public string FileName { get; set; } = string.Empty;

        // MIME type as reported at upload, used to pick an icon and to set the
        // Content-Type when streaming the file back.
        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        // The file bytes. Mapped to bytea; never selected in list queries.
        public byte[] Content { get; set; } = [];

        // Email of the admin/therapist who uploaded it, for a light audit trail.
        public string? UploadedByEmail { get; set; }

        public PatientAttachment() { }

        public PatientAttachment(Guid patientId, string fileName, string contentType, long sizeBytes, byte[] content, string? uploadedByEmail)
        {
            PatientAttachmentId = Guid.NewGuid();
            PatientId = patientId;
            FileName = fileName;
            ContentType = contentType;
            SizeBytes = sizeBytes;
            Content = content;
            UploadedByEmail = uploadedByEmail;
        }
    }
}
