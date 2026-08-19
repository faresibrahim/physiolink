using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.Exceptions;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class PatientAttachmentService : IPatientAttachmentService
    {
        // The user-facing file-size cap. 10 MB comfortably covers scans and PDFs
        // while keeping row sizes sane for bytea storage. Enforced on the actual
        // file bytes below, so this is the message a too-large upload gets.
        public const long MaxFileSizeBytes = 10 * 1024 * 1024;

        // The HTTP request-size limit for the upload endpoints. Larger than the file
        // cap so the multipart boundary overhead on a near-limit file doesn't trip a
        // raw 413 before the friendly MaxFileSizeBytes validation can run.
        public const long MaxUploadRequestBytes = 12 * 1024 * 1024;

        // Whitelist by content type — documents and images a clinic would attach to a
        // record. Anything executable/unknown is refused rather than stored blind.
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/png",
            "image/jpeg",
            "image/gif",
            "image/webp",
            "image/heic",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain",
            "text/csv",
        };

        private readonly PhysioLinkDbContext _dbContext;
        private readonly ICurrentClinicService _currentClinicService;

        public PatientAttachmentService(PhysioLinkDbContext dbContext, ICurrentClinicService currentClinicService)
        {
            _dbContext = dbContext;
            _currentClinicService = currentClinicService;
        }

        public async Task<List<PatientAttachmentDto>?> GetForPatientAsync(Guid patientId)
        {
            // The clinic query filter on Patients makes this null for a patient in
            // another clinic, so we never list attachments across the tenant boundary.
            var patientExists = await _dbContext.Patients
                .AsNoTracking()
                .AnyAsync(p => p.PatientId == patientId);
            if (!patientExists) return null;

            // Project WITHOUT Content so we never pull file bytes into a list query.
            return await _dbContext.PatientAttachments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new PatientAttachmentDto
                {
                    PatientAttachmentId = a.PatientAttachmentId,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    SizeBytes = a.SizeBytes,
                    UploadedByEmail = a.UploadedByEmail,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<PatientAttachmentDto?> UploadAsync(
            Guid patientId, string fileName, string contentType, byte[] content, string? uploadedByEmail)
        {
            if (content.Length == 0)
                throw new AttachmentValidationException("The file is empty.");

            if (content.Length > MaxFileSizeBytes)
                throw new AttachmentValidationException(
                    $"The file exceeds the {MaxFileSizeBytes / (1024 * 1024)} MB limit.");

            if (!AllowedContentTypes.Contains(contentType))
                throw new AttachmentValidationException("That file type isn't allowed.");

            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            // Confirm the patient is in this clinic before writing anything under it —
            // the query filter makes this false for a foreign patient.
            var patientExists = await _dbContext.Patients
                .AsNoTracking()
                .AnyAsync(p => p.PatientId == patientId);
            if (!patientExists) return null;

            var safeName = SanitizeFileName(fileName);

            var attachment = new PatientAttachment(patientId, safeName, contentType, content.Length, content, uploadedByEmail)
            {
                ClinicId = clinicId
            };

            _dbContext.PatientAttachments.Add(attachment);
            await _dbContext.SaveChangesAsync();

            return new PatientAttachmentDto
            {
                PatientAttachmentId = attachment.PatientAttachmentId,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                SizeBytes = attachment.SizeBytes,
                UploadedByEmail = attachment.UploadedByEmail,
                CreatedAt = attachment.CreatedAt
            };
        }

        public async Task<PatientAttachmentContentDto?> GetContentAsync(Guid patientId, Guid attachmentId)
        {
            return await _dbContext.PatientAttachments
                .AsNoTracking()
                .Where(a => a.PatientAttachmentId == attachmentId && a.PatientId == patientId)
                .Select(a => new PatientAttachmentContentDto
                {
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    Content = a.Content
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(Guid patientId, Guid attachmentId)
        {
            var attachment = await _dbContext.PatientAttachments
                .FirstOrDefaultAsync(a => a.PatientAttachmentId == attachmentId && a.PatientId == patientId);
            if (attachment == null) return false;

            // Soft delete — the global IsDeleted filter hides it, but the bytes are
            // retained (mirrors "data is retained" on patient deactivation).
            attachment.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Strip any path the browser may have sent and cap the length, so a crafted
        // filename can't traverse or bloat. Used for both storage and download naming.
        private static string SanitizeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(name)) name = "attachment";
            return name.Length > 260 ? name[^260..] : name;
        }
    }
}
