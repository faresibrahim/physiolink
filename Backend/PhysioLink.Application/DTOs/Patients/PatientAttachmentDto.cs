namespace PhysioLink.Application.DTOs.Patients
{
    // Attachment metadata (no bytes) — for listing a patient's files.
    public class PatientAttachmentDto
    {
        public Guid PatientAttachmentId { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string? UploadedByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Attachment with bytes — for streaming a download.
    public class PatientAttachmentContentDto
    {
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required byte[] Content { get; set; }
    }
}
