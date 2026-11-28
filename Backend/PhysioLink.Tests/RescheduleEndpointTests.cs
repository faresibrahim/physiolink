using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.API.Controllers;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.DTOs.Slots;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Tests;

// Pins the HTTP status each reschedule outcome maps to; the patient app keys its
// messages off these codes (409 = slot gone, 422 = can't reschedule in the app).
public class RescheduleEndpointTests
{
    [Theory]
    [InlineData(RescheduleOutcome.Requested, 201)]
    [InlineData(RescheduleOutcome.Moved, 200)]
    [InlineData(RescheduleOutcome.NotFound, 404)]
    [InlineData(RescheduleOutcome.SlotUnavailable, 409)]
    [InlineData(RescheduleOutcome.TooLate, 422)]
    [InlineData(RescheduleOutcome.NotReschedulable, 422)]
    public async Task Outcome_Maps_To_Status_Code(RescheduleOutcome outcome, int expected)
    {
        var controller = new PatientBookingController(new FakeSlotService(outcome))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "TestAuth"))
                }
            }
        };

        var result = await controller.Reschedule(Guid.NewGuid(), new RescheduleAppointmentDto { SlotId = Guid.NewGuid() }, default);

        var status = result switch
        {
            ObjectResult o => o.StatusCode,
            StatusCodeResult s => s.StatusCode,
            _ => null
        };
        Assert.Equal(expected, status);
    }

    private sealed class FakeSlotService(RescheduleOutcome outcome) : IPatientSlotService
    {
        public Task<RescheduleResult> RescheduleAsync(Guid applicationUserId, Guid appointmentId, RescheduleAppointmentDto request, CancellationToken ct = default) =>
            Task.FromResult(RescheduleResult.Of(outcome, new PatientAppointmentDto()));

        public Task<List<PatientSlotDto>> GetMySlotsAsync(Guid applicationUserId, DateTime? from, DateTime? to, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<RequestSlotResult> RequestSlotAsync(Guid applicationUserId, RequestSlotDto request, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<List<PatientAppointmentDto>> GetMyAppointmentsAsync(Guid applicationUserId, CancellationToken ct = default) =>
            throw new NotImplementedException();
    }
}
