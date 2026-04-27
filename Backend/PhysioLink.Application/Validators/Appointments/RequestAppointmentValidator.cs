using FluentValidation;
using PhysioLink.Application.DTOs.Appointments;

namespace PhysioLink.Application.Validators.Appointments
{
    public class AppointmentValidator : AbstractValidator<AppointmentRequestDto>
    {
        public AppointmentValidator()
        {
            RuleFor(x => x.PatientId)
                .NotEmpty();

            RuleFor(x => x.AppointmentTime)
                .GreaterThan(DateTime.UtcNow);

            // Notes is optional, but cap it if provided
            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .When(x => x.Notes != null);
        }
    }
}
