using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhysioLink.Domain.Entities;

namespace PhysioLink.Infrastructure.Data.Configurations
{
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.ToTable("Exercises");
            builder.HasKey(p=>p.ExerciseId);

            builder.Property(p=>p.Name)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(p=>p.Description)
            .IsRequired()
            .HasMaxLength(1000);

            builder.Property(p=>p.DescriptionAr)
            .HasMaxLength(1000);

            builder.Property(p=>p.Difficulty)
            .IsRequired();

            builder.Property(p=>p.Category)
            .IsRequired();

            builder.Property(p=>p.VideoUrl)
            .HasMaxLength(500);
        }
    }
}