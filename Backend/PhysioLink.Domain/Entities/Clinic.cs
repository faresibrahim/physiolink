namespace PhysioLink.Domain.Entities
{
    public class Clinic : AuditableEntity
    {
        public Guid ClinicId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set;}

        public Clinic(string name, string address, string phoneNumber, string email, bool isActive)
        {
            ClinicId = Guid.NewGuid();
            Name = name;
            Address = address;
            PhoneNumber = phoneNumber;
            Email = email;
            IsActive = isActive;
        }
    }
}