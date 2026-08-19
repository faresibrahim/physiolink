using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PhysioLink.Domain.Entities;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Infrastructure.Data
{
    public class PhysioLinkDbContext : DbContext
    {
        // Resolved ONCE per request, at construction. The clinic-scoped query filter
        // references THIS field (rooted on the DbContext instance), so EF Core re-reads
        // it from the executing context on every query — even though the model is compiled
        // once at startup and cached app-wide. Capturing the clinic *service* as an
        // Expression.Constant instead froze one request's clinic scope into the cached
        // model, which leaked every clinic's data to every other clinic.
        private readonly Guid? _currentClinicId;

        // Cached FieldInfo for _currentClinicId, used to build a context-rooted member
        // access in the query filter (Expression.Field needs the non-public field).
        private static readonly FieldInfo CurrentClinicIdField =
            typeof(PhysioLinkDbContext).GetField(
                nameof(_currentClinicId), BindingFlags.NonPublic | BindingFlags.Instance)!;

        public PhysioLinkDbContext(DbContextOptions<PhysioLinkDbContext> options, ICurrentClinicService currentClinicService) : base(options)
        {
            _currentClinicId = currentClinicService.GetCurrentClinicId();
        }

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<ExerciseAssignment> ExerciseAssignments => Set<ExerciseAssignment>();
        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<Therapist> Therapists => Set<Therapist>();
        public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
        public DbSet<PatientAttachment> PatientAttachments => Set<PatientAttachment>();

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
            // Root the current clinic id on THIS context instance. This produces the same
            // expression shape as a `e => EF.Property<Guid?>(e,"ClinicId") == _currentClinicId`
            // lambda would, which EF Core re-parameterizes against the executing context on
            // every query. Never capture the clinic id (or its service) as an Expression.Constant
            // here — the model is cached at startup and a constant would pin one clinic forever.
            var currentClinicId = Expression.Field(Expression.Constant(this), CurrentClinicIdField);
            var body = Expression.AndAlso(
                Expression.Equal(isDeleted, Expression.Constant(false)),
                Expression.Equal(clinicId, currentClinicId));
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
