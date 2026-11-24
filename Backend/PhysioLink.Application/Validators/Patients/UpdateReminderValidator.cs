using FluentValidation;
using PhysioLink.Application.Common;
using PhysioLink.Application.DTOs.Patients;

namespace PhysioLink.Application.Validators.Patients
{
    public class UpdateReminderValidator : AbstractValidator<UpdateReminderDto>
    {
        public UpdateReminderValidator()
        {
            RuleFor(x => x.Time)
                .NotEmpty()
                .Must(t => ReminderTimeFormat.TryParse(t, out _))
                .WithMessage("Time must be a 24-hour HH:mm value between 00:00 and 23:59.");
        }
    }
}
