

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

    public string? DeviceToken { get; set; }

    // Daily exercise reminder. ReminderTime is a CLINIC-local wall-clock time
    // (see Application/Common/ClinicTime), never UTC — the patient picks "18:00"
    // meaning 18:00 in the clinic, whatever the DST offset is that day.
    public static readonly TimeOnly DefaultReminderTime = new(18, 0);

    public bool ReminderEnabled { get; set; }
    public TimeOnly ReminderTime { get; set; } = DefaultReminderTime;

    // CLINIC-local date the reminder was last handled (sent, or deliberately
    // skipped). Guarantees at most one reminder per patient per clinic day.
    public DateOnly? LastReminderSentOn { get; set; }

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