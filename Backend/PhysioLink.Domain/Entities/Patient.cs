

namespace PhysioLink.Domain.Entities{
public class Patient : ClinicScopedEntity
{
    public Guid PatientId { get; set;}
    public string FirstName { get; set; }

    public string LastName { get; set;}
    public string PhoneNumber { get; set;}

    public Guid ApplicationUserId {get; set;}
    public Guid? TherapistId { get; set; }

    public Therapist? Therapist { get; set; }
    public Clinic? Clinic { get; set; }

       public string? Email { get; set;}
    public string Username { get; set; }
    public string Diagnosis { get; set;}

    public bool IsActive { get; set; }

    public Patient(string firstName, string lastName, string phoneNumber, Guid applicationUserId, string username, string? email, string diagnosis)
        {
            PatientId = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Email = email;
            Diagnosis = diagnosis;
            PhoneNumber = phoneNumber;
            ApplicationUserId = applicationUserId;
            IsActive = true;
        }
    
}
}