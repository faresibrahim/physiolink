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

        // Seeded operating-hours window (spec D11 / 1.5). The slot grid renders one
        // row per hour in [OpenHour, CloseHour). No settings UI — seeded only.
        public int OpenHour { get; set; }
        public int CloseHour { get; set; }

        public Clinic(string name, string address, string phoneNumber, string email, bool isActive, int openHour = 8, int closeHour = 18)
        {
            ClinicId = Guid.NewGuid();
            Name = name;
            Address = address;
            PhoneNumber = phoneNumber;
            Email = email;
            IsActive = isActive;
            OpenHour = openHour;
            CloseHour = closeHour;
        }
    }
}
