using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.Application.DTOs.Slots;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.Controllers.Admin
{
    [Authorize(Roles = "ClinicAdmin")]
    [ApiController]
    [Route("api/v1/admin/therapists/{therapistId:guid}/slots")]
    public class AdminSlotsController : ControllerBase
    {
        private readonly IAdminSlotService _adminSlotService;

        public AdminSlotsController(IAdminSlotService adminSlotService)
        {
            _adminSlotService = adminSlotService;
        }

        // GET /api/v1/admin/therapists/{therapistId}/slots?weekStart=YYYY-MM-DD
        [HttpGet]
        public async Task<IActionResult> GetWeek(Guid therapistId, [FromQuery] DateTime? weekStart)
        {
            // Default to the current week (Monday-anchored) if none supplied.
            var start = weekStart?.Date ?? StartOfWeek(DateTime.UtcNow.Date);
            var grid = await _adminSlotService.GetWeekGridAsync(therapistId, start);
            if (grid == null) return NotFound();
            return Ok(grid);
        }

        // GET /api/v1/admin/therapists/{therapistId}/slots/available?from=&to=
        // Available future slots — the options for the manual "New Appointment" modal.
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(Guid therapistId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var slots = await _adminSlotService.GetAvailableSlotsAsync(therapistId, from, to);
            if (slots == null) return NotFound();
            return Ok(slots);
        }

        // POST /api/v1/admin/therapists/{therapistId}/slots  { scheduledAt }
        [HttpPost]
        public async Task<IActionResult> Create(Guid therapistId, [FromBody] CreateSlotDto dto)
        {
            var outcome = await _adminSlotService.CreateSlotAsync(therapistId, dto.ScheduledAt);
            return outcome switch
            {
                // Idempotent toggle-on: friendlier than 409 for a toggle (spec 2.2 step 4).
                SlotWriteOutcome.Ok => Ok(),
                SlotWriteOutcome.TherapistNotFound => NotFound(),
                SlotWriteOutcome.NotOnTheHour => BadRequest("Slot time must be exactly on the hour."),
                SlotWriteOutcome.OutsideOperatingWindow => BadRequest("Slot time is outside the clinic's operating hours."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        // DELETE /api/v1/admin/therapists/{therapistId}/slots?scheduledAt=...
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid therapistId, [FromQuery] DateTime scheduledAt)
        {
            var outcome = await _adminSlotService.DeleteSlotAsync(therapistId, scheduledAt);
            return outcome switch
            {
                SlotWriteOutcome.Ok => NoContent(),
                SlotWriteOutcome.SlotNotFound => NotFound(),
                SlotWriteOutcome.SlotIsLive => Conflict("Slot has a live request/booking; cancel it first."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            // Monday as the first column.
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff);
        }
    }
}
