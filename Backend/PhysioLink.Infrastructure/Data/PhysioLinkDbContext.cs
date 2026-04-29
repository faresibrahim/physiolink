using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PhysioLink.Domain.Entities;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Infrastructure.Data
{
    public class PhysioLinkDbContext : DbContext
    {
        private readonly ICurrentClinicService _currentClinicService;

        public PhysioLinkDbContext(DbContextOptions<PhysioLinkDbContext> options, ICurrentClinicService currentClinicService) : base(options)
        {
            _currentClinicService = currentClinicService;
        }

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<ExerciseAssignment> ExerciseAssignments => Set<ExerciseAssignment>();
        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<Therapist> Therapists => Set<Therapist>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PhysioLinkDbContext).Assembly);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ClinicScopedEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(BuildClinicScopedFilter(entityType.ClrType));
                }
                else if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(BuildIsDeletedFilter(entityType.ClrType));
                }
            }
           
        }

        private LambdaExpression BuildIsDeletedFilter(Type entityType)
        {
            var param = Expression.Parameter(entityType, "e");
            var isDeleted = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { typeof(bool) },
                param, Expression.Constant("IsDeleted"));
            var body = Expression.Equal(isDeleted, Expression.Constant(false));
            return Expression.Lambda(body, param);
        }

        private LambdaExpression BuildClinicScopedFilter(Type entityType)
        {
            var param = Expression.Parameter(entityType, "e");
            var isDeleted = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { typeof(bool) },
                param, Expression.Constant("IsDeleted"));
            var clinicId = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { typeof(Guid?) },
                param, Expression.Constant("ClinicId"));
            var getCurrentClinicId = Expression.Call(
                Expression.Constant(_currentClinicService),
                typeof(ICurrentClinicService).GetMethod(nameof(ICurrentClinicService.GetCurrentClinicId))!);
            var body = Expression.AndAlso(
                Expression.Equal(isDeleted, Expression.Constant(false)),
                Expression.Equal(clinicId, getCurrentClinicId));
            return Expression.Lambda(body, param);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
