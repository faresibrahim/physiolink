using FluentValidation;
using PhysioLink.Application.DTOs.Slots;

namespace PhysioLink.Application.Validators.Appointments
{
    public class RequestSlotValidator : AbstractValidator<RequestSlotDto>
    {
        public RequestSlotValidator()
        {
            RuleFor(x => x.SlotId)
                .NotEmpty();

            RuleFor(x => x.Type)
                .MaximumLength(200)
                .When(x => x.Type != null);

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .When(x => x.Notes != null);
        }
    }
}
