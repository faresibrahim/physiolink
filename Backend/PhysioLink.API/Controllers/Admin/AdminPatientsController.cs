using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.Exceptions;
using PhysioLink.Application.Interfaces;
using PhysioLink.Infrastructure.Services;

namespace PhysioLink.API.Controllers.Admin
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1/admin/patients")]
    public class AdminPatientsController : ControllerBase
    {
        private readonly IAdminPatientService _adminPatientService;
        private readonly IPatientAttachmentService _attachmentService;

        public AdminPatientsController(
            IAdminPatientService adminPatientService,
            IPatientAttachmentService attachmentService)
        {
            _adminPatientService = adminPatientService;
            _attachmentService = attachmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10, Guid? therapistId = null, [FromQuery] string? search = null)
        {
            var result = await _adminPatientService.GetAllAsync(page, pageSize, therapistId, search);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _adminPatientService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto createPatientDto)
        {
            try
            {
                var result = await _adminPatientService.CreateAsync(createPatientDto);
                return CreatedAtAction(nameof(GetById), new { id = result.PatientId }, result);
            }
            catch (EmailInUseException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDto updatePatientDto)
        {
            var result = await _adminPatientService.UpdateAsync(id, updatePatientDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _adminPatientService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // ── Attachments ──────────────────────────────────────────────────────

        [HttpGet("{id:guid}/attachments")]
        public async Task<IActionResult> GetAttachments(Guid id)
        {
            var result = await _attachmentService.GetForPatientAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("{id:guid}/attachments")]
        [RequestSizeLimit(PatientAttachmentService.MaxUploadRequestBytes)]
        public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file was uploaded." });

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var uploadedBy = User.FindFirstValue(ClaimTypes.Email);

            try
            {
                var result = await _attachmentService.UploadAsync(
                    id, file.FileName, file.ContentType, stream.ToArray(), uploadedBy);
                if (result == null) return NotFound();
                return CreatedAtAction(nameof(GetAttachments), new { id }, result);
            }
            catch (AttachmentValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
        public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId)
        {
            var content = await _attachmentService.GetContentAsync(id, attachmentId);
            if (content == null) return NotFound();
            return File(content.Content, content.ContentType, content.FileName);
        }

        [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
        public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
        {
            var result = await _attachmentService.DeleteAsync(id, attachmentId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
