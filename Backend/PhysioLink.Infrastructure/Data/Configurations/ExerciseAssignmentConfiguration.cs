using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class ExerciseAssignmentConfiguration : IEntityTypeConfiguration<ExerciseAssignment>
    {
        public void Configure(EntityTypeBuilder<ExerciseAssignment> builder)
        {
            builder.ToTable("ExerciseAssignments");
            builder.HasKey(p=>p.ExerciseAssignmentId);

            builder.Property(p=>p.PatientId)
            .IsRequired();
            
            builder.Property(p=>p.TherapistId)
            .IsRequired();
            
            
            builder.Property(p=>p.Sets)
            .IsRequired();
            

            
            builder.Property(p=>p.Reps)
            .IsRequired();
            

             builder.Property(p=>p.DurationMinutes)
            .IsRequired();
            

            
             builder.Property(p=>p.AssignedAt)
            .IsRequired();            
            

        }
    }
}