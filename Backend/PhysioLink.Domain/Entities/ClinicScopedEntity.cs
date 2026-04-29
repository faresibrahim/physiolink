namespace PhysioLink.Domain.Entities
{
    public abstract class ClinicScopedEntity : AuditableEntity
    {
        public Guid ClinicId { get; set; }
        
    }
}