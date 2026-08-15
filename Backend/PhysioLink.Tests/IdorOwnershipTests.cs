using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.API.Controllers;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.DTOs.Exercises;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.DTOs.Profile;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Tests;

// Regression tests for the S1–S3 IDOR fixes: a patient may only read/act on their
// own records. The patient is resolved from the JWT identity (ApplicationUserId ->
// PatientId), never trusted from the URL or request body. A route/body id that isn't
// the caller's must be rejected. These are controller-level tests with hand-rolled
// fakes (no DB) so they pin the authorization decision directly.
public class IdorOwnershipTests
{
    private static readonly Guid CallerUserId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CallerPatientId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid OtherPatientId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");

    // Builds a ControllerContext whose User carries the caller's ApplicationUserId as
    // the NameIdentifier claim ('sub' after inbound mapping), matching a real request.
    private static ControllerContext ContextForCaller()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, CallerUserId.ToString()),
        }, "TestAuth");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    // ---- S1: profile / progress / exercises are scoped to the caller ---------------

    [Fact]
    public async Task GetProfile_WithCallersOwnId_ReturnsOk()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var controller = new PatientController(patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var result = await controller.GetPatientProfile(CallerPatientId);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetProfile_WithAnotherPatientsId_IsForbidden()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var controller = new PatientController(patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var result = await controller.GetPatientProfile(OtherPatientId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_WithAnotherPatientsId_IsForbidden()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var controller = new PatientController(patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var request = new UpdatePatientProfileDto
        {
            FirstName = "Mallory",
            LastName = "Evil",
            PhoneNumber = "000",
            Diagnosis = "n/a"
        };

        var result = await controller.UpdatePatientProfile(OtherPatientId, request);

        Assert.IsType<ForbidResult>(result);
        Assert.False(patientService.UpdateWasCalled); // never reached the service
    }

    [Fact]
    public async Task GetProgress_WithAnotherPatientsId_IsForbidden()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var controller = new PatientController(patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var result = await controller.GetPatientProgress(OtherPatientId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetExercises_WithAnotherPatientsId_IsForbidden()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var exerciseService = new FakeExerciseService();
        var controller = new ExerciseController(exerciseService, patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var result = await controller.GetPatientExercises(OtherPatientId);

        Assert.IsType<ForbidResult>(result);
    }

    // ---- S3: feedback is scoped to the caller's own assignment ---------------------

    [Fact]
    public async Task SubmitFeedback_PassesCallersPatientId_ToService()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var exerciseService = new FakeExerciseService();
        var controller = new ExerciseController(exerciseService, patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var assignmentId = Guid.NewGuid();
        await controller.SubmitFeedbackLevel(assignmentId, new SubmitFeedbackDto { Rating = 3 });

        // The controller must forward the caller's own PatientId so the repository can
        // reject an assignment that belongs to someone else.
        Assert.Equal(CallerPatientId, exerciseService.LastCallerPatientId);
    }

    [Fact]
    public async Task SubmitFeedback_WhenCallerHasNoPatientRecord_IsForbidden()
    {
        var patientService = new FakePatientService(null); // resolves to no patient
        var exerciseService = new FakeExerciseService();
        var controller = new ExerciseController(exerciseService, patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var result = await controller.SubmitFeedbackLevel(Guid.NewGuid(), new SubmitFeedbackDto { Rating = 3 });

        Assert.IsType<ForbidResult>(result);
    }

    // ---- S2: appointment creation binds to the caller, ignoring the body -----------

    [Fact]
    public async Task CreateAppointment_OverridesBodyPatientId_WithCaller()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var appointmentService = new FakeAppointmentService();
        var controller = new AppointmentController(appointmentService, patientService)
        {
            ControllerContext = ContextForCaller()
        };

        // Attacker supplies someone else's PatientId in the body.
        var request = new AppointmentRequestDto
        {
            PatientId = OtherPatientId,
            AppointmentTime = DateTime.UtcNow.AddDays(1)
        };

        await controller.CreateAppointment(request);

        // The service must have received the caller's own id, not the body's.
        Assert.Equal(CallerPatientId, appointmentService.LastRequest!.PatientId);
    }

    [Fact]
    public async Task GetAppointments_WithAnotherPatientsId_IsForbidden()
    {
        var patientService = new FakePatientService(CallerPatientId);
        var appointmentService = new FakeAppointmentService();
        var controller = new AppointmentController(appointmentService, patientService)
        {
            ControllerContext = ContextForCaller()
        };

        var result = await controller.GetPatientAppointments(OtherPatientId);

        Assert.IsType<ForbidResult>(result);
    }
}

// ---- Hand-rolled fakes (no mocking library in this project) --------------------------

internal class FakePatientService : IPatientService
{
    private readonly Guid? _patientId;
    public bool UpdateWasCalled { get; private set; }

    public FakePatientService(Guid? patientId) => _patientId = patientId;

    public Task<Guid?> ResolvePatientIdAsync(Guid applicationUserId) => Task.FromResult(_patientId);

    public Task<PatientProfileDto> GetPatientProfileAsync(Guid patientId) =>
        Task.FromResult(new PatientProfileDto { PatientId = patientId });

    public Task<bool> UpdatePatientProfileAsync(Guid patientId, UpdatePatientProfileDto request)
    {
        UpdateWasCalled = true;
        return Task.FromResult(true);
    }

    public Task<PatientProgressDto> GetPatientProgressAsync(Guid patientId) =>
        Task.FromResult(new PatientProgressDto());
}

internal class FakeExerciseService : IExerciseService
{
    public Guid? LastCallerPatientId { get; private set; }

    public Task<PagedResult<AssignedExerciseDto>> GetPatientExercisesAsync(Guid patientId, int page, int pageSize) =>
        Task.FromResult(new PagedResult<AssignedExerciseDto>());

    public Task<bool> SubmitFeedbackAsync(Guid id, SubmitFeedbackDto request, Guid callerPatientId)
    {
        LastCallerPatientId = callerPatientId;
        return Task.FromResult(true);
    }
}

internal class FakeAppointmentService : IAppointmentService
{
    public AppointmentRequestDto? LastRequest { get; private set; }

    public Task<PagedResult<AppointmentDto>> GetPatientAppointmentAsync(Guid patientId, int page, int pageSize) =>
        Task.FromResult(new PagedResult<AppointmentDto>());

    public Task<AppointmentDto> CreateAppointmentAsync(AppointmentRequestDto request)
    {
        LastRequest = request;
        return Task.FromResult(new AppointmentDto { AppointmentId = Guid.NewGuid() });
    }
}
