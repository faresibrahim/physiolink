using FluentValidation;
using PhysioLink.Application.DTOs.Patients;

namespace PhysioLink.Application.Validators.Patients
{
    public class UpdatePatientProfileValidator : AbstractValidator<UpdatePatientProfileDto>
    {
        public UpdatePatientProfileValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Diagnosis).NotEmpty().MaximumLength(500);
        }
    }
}