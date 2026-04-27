

namespace PhysioLink.Domain.Entities{
public class Patient : AuditableEntity
{
    public Guid PatientId { get; set;}
    public string FirstName { get; set; }

    public string LastName { get; set;}
    public string PhoneNumber { get; set;}

    public Guid ApplicationUserId {get; set;}

    public string Email { get; set;}
    public string Diagnosis { get; set;}

    public bool IsActive { get; set; } 

    public Patient(string firstName, string lastName, string phoneNumber, Guid applicationUserId, string email, string diagnosis)
        {
            PatientId = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Diagnosis = diagnosis;
            PhoneNumber = phoneNumber;
            ApplicationUserId = applicationUserId;
            IsActive = true;
        }
    
}
}