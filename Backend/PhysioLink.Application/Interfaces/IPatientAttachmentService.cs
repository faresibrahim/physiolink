using PhysioLink.Application.DTOs.Patients;

namespace PhysioLink.Application.Interfaces
{
    public interface IPatientAttachmentService
    {
        // Metadata for every (non-deleted) attachment on a patient, newest first.
        // Returns null when the patient isn't visible to the current clinic.
        Task<List<PatientAttachmentDto>?> GetForPatientAsync(Guid patientId);

        // Stores a file against the patient. Throws AttachmentValidationException on
        // a bad file; returns null when the patient isn't visible to the current clinic.
        Task<PatientAttachmentDto?> UploadAsync(
            Guid patientId, string fileName, string contentType, byte[] content, string? uploadedByEmail);

        // Bytes + name/type for a download, or null if not found in this clinic.
        Task<PatientAttachmentContentDto?> GetContentAsync(Guid patientId, Guid attachmentId);

        // Soft-deletes the attachment. False when not found in this clinic.
        Task<bool> DeleteAsync(Guid patientId, Guid attachmentId);
    }
}
