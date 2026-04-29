using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;

namespace PhysioLink.Infrastructure.Data
{
    public class DbSeeder
    {
        private static readonly Guid Clinic1Id = new Guid("11111111-1111-1111-1111-111111111111");
        private static readonly Guid Clinic2Id = new Guid("22222222-2222-2222-2222-222222222222");

        private static readonly Guid C1T1Id = new Guid("11111111-1111-1111-1111-100000000001");
        private static readonly Guid C1T2Id = new Guid("11111111-1111-1111-1111-100000000002");
        private static readonly Guid C1T3Id = new Guid("11111111-1111-1111-1111-100000000003");

        private static readonly Guid C2T1Id = new Guid("22222222-2222-2222-2222-200000000001");
        private static readonly Guid C2T2Id = new Guid("22222222-2222-2222-2222-200000000002");
        private static readonly Guid C2T3Id = new Guid("22222222-2222-2222-2222-200000000003");

        public static async Task SeedAsync(IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PhysioLinkDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

            if (!context.Users.Any())
            {
                var hashedPassword = passwordHasher.HashPassword(null!, "test@123");
                var therapist = new ApplicationUser(
                    firstName: "Fares",
                    lastName: "Ibrahim",
                    email: "fares.a.ibrahim@gmail.com",
                    passwordHash: hashedPassword
                );
                context.Users.Add(therapist);
                await context.SaveChangesAsync();
            }

            if (!context.Exercises.Any())
            {
                context.Exercises.AddRange(
                    new Exercise(name: "Knee Extension", reps: 15, sets: 3, durationMinutes: 10,
                        description: "Sit on a chair and slowly extend your knee to full straightness, hold for 2 seconds, then lower.",
                        difficulty: DifficultyLevel.Moderate),
                    new Exercise(name: "Hip Abduction", reps: 12, sets: 3, durationMinutes: 15,
                        description: "Lie on your side and raise your top leg to 45 degrees, hold for 3 seconds, then lower slowly.",
                        difficulty: DifficultyLevel.Hard),
                    new Exercise(name: "Single Leg Balance", reps: 10, sets: 4, durationMinutes: 20,
                        description: "Stand on one leg with slight knee bend, maintain balance for 30 seconds, switch legs.",
                        difficulty: DifficultyLevel.Hard)
                );
                await context.SaveChangesAsync();
            }

            if (!context.Patients.IgnoreQueryFilters().Any(p => p.Email == "john.smith@example.com"))
            {
                var hashedPassword = passwordHasher.HashPassword(null!, "patient@123");
                var patientUser = new ApplicationUser(
                    firstName: "John",
                    lastName: "Smith",
                    email: "john.smith@example.com",
                    passwordHash: hashedPassword
                );
                context.Users.Add(patientUser);

                var patient = new Patient(
                    firstName: "John",
                    lastName: "Smith",
                    phoneNumber: "+970599111111",
                    applicationUserId: patientUser.ApplicationUserId,
                    email: "john.smith@example.com",
                    diagnosis: "Knee injury"
                );
                patient.ClinicId = Clinic1Id;
                context.Patients.Add(patient);
                await context.SaveChangesAsync();

                var therapist = await context.Users.IgnoreQueryFilters().FirstAsync(u => u.Email == "fares.a.ibrahim@gmail.com");
                var exercises = await context.Exercises.IgnoreQueryFilters().ToListAsync();

                foreach (var exercise in exercises)
                {
                    context.ExerciseAssignments.Add(new ExerciseAssignment(
                        therapistName: therapist.FirstName + " " + therapist.LastName,
                        patientId: patient.PatientId,
                        exerciseId: exercise.ExerciseId,
                        sets: exercise.Sets,
                        reps: exercise.Reps,
                        durationMinutes: exercise.DurationMinutes,
                        scheduledDate: DateTime.UtcNow.AddDays(1),
                        frequencyPerWeek: 3,
                        assignedAt: DateTime.UtcNow
                    ));
                }
                await context.SaveChangesAsync();
            }
            else
            {
                var johnSmith = await context.Patients.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Email == "john.smith@example.com");
                if (johnSmith != null && johnSmith.ClinicId == Guid.Empty)
                {
                    johnSmith.ClinicId = Clinic1Id;
                    await context.SaveChangesAsync();
                }
            }

            if (!context.Clinics.IgnoreQueryFilters().Any(c => c.ClinicId == Clinic2Id))
            {
                var clinic2 = new Clinic("PhysioPlus Center", "456 Health Ave", "+97000000002", "clinic2@physiolink.com", true);
                clinic2.ClinicId = Clinic2Id;
                context.Clinics.Add(clinic2);
                await context.SaveChangesAsync();
            }

            if (!context.Therapists.IgnoreQueryFilters().Any())
            {
                var t1 = new Therapist(Clinic1Id, "Sarah", "Johnson", "sarah.johnson@clinic1.com", "+970599200001", "Orthopedic Physiotherapy", true);
                t1.TherapistId = C1T1Id;
                var t2 = new Therapist(Clinic1Id, "Michael", "Brown", "michael.brown@clinic1.com", "+970599200002", "Sports Rehabilitation", true);
                t2.TherapistId = C1T2Id;
                var t3 = new Therapist(Clinic1Id, "Emily", "Davis", "emily.davis@clinic1.com", "+970599200003", "Neurological Physiotherapy", true);
                t3.TherapistId = C1T3Id;

                var t4 = new Therapist(Clinic2Id, "James", "Wilson", "james.wilson@clinic2.com", "+970599200004", "Pediatric Physiotherapy", true);
                t4.TherapistId = C2T1Id;
                var t5 = new Therapist(Clinic2Id, "Linda", "Martinez", "linda.martinez@clinic2.com", "+970599200005", "Geriatric Physiotherapy", true);
                t5.TherapistId = C2T2Id;
                var t6 = new Therapist(Clinic2Id, "Robert", "Taylor", "robert.taylor@clinic2.com", "+970599200006", "Post-Surgical Rehabilitation", true);
                t6.TherapistId = C2T3Id;

                context.Therapists.AddRange(t1, t2, t3, t4, t5, t6);
                await context.SaveChangesAsync();
            }

            if (!context.Patients.IgnoreQueryFilters().Any(p => p.Email == "alice.cooper@example.com"))
            {
                var c1Users = new[]
                {
                    new ApplicationUser("Alice", "Cooper", "alice.cooper@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                    new ApplicationUser("Bob", "Williams", "bob.williams@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                    new ApplicationUser("Carol", "Jones", "carol.jones@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                    new ApplicationUser("David", "Lee", "david.lee@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                };
                context.Users.AddRange(c1Users);
                await context.SaveChangesAsync();

                var c1Patients = new[]
                {
                    new Patient("Alice", "Cooper", "+970599300001", c1Users[0].ApplicationUserId, "alice.cooper@example.com", "Shoulder impingement") { ClinicId = Clinic1Id, TherapistId = C1T1Id },
                    new Patient("Bob", "Williams", "+970599300002", c1Users[1].ApplicationUserId, "bob.williams@example.com", "Lower back pain") { ClinicId = Clinic1Id, TherapistId = C1T2Id },
                    new Patient("Carol", "Jones", "+970599300003", c1Users[2].ApplicationUserId, "carol.jones@example.com", "ACL recovery") { ClinicId = Clinic1Id, TherapistId = C1T3Id },
                    new Patient("David", "Lee", "+970599300004", c1Users[3].ApplicationUserId, "david.lee@example.com", "Rotator cuff tear") { ClinicId = Clinic1Id, TherapistId = C1T1Id },
                };
                context.Patients.AddRange(c1Patients);
                await context.SaveChangesAsync();
            }

            if (!context.Patients.IgnoreQueryFilters().Any(p => p.Email == "emma.clark@example.com"))
            {
                var c2Users = new[]
                {
                    new ApplicationUser("Emma", "Clark", "emma.clark@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                    new ApplicationUser("Frank", "Lewis", "frank.lewis@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                    new ApplicationUser("Grace", "Hall", "grace.hall@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                    new ApplicationUser("Henry", "Young", "henry.young@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                    new ApplicationUser("Iris", "King", "iris.king@example.com", passwordHasher.HashPassword(null!, "patient@123")),
                };
                context.Users.AddRange(c2Users);
                await context.SaveChangesAsync();

                var c2Patients = new[]
                {
                    new Patient("Emma", "Clark", "+970599400001", c2Users[0].ApplicationUserId, "emma.clark@example.com", "Hip fracture recovery") { ClinicId = Clinic2Id, TherapistId = C2T1Id },
                    new Patient("Frank", "Lewis", "+970599400002", c2Users[1].ApplicationUserId, "frank.lewis@example.com", "Cerebral palsy") { ClinicId = Clinic2Id, TherapistId = C2T1Id },
                    new Patient("Grace", "Hall", "+970599400003", c2Users[2].ApplicationUserId, "grace.hall@example.com", "Chronic back pain") { ClinicId = Clinic2Id, TherapistId = C2T2Id },
                    new Patient("Henry", "Young", "+970599400004", c2Users[3].ApplicationUserId, "henry.young@example.com", "Post-knee replacement") { ClinicId = Clinic2Id, TherapistId = C2T3Id },
                    new Patient("Iris", "King", "+970599400005", c2Users[4].ApplicationUserId, "iris.king@example.com", "Ankle sprain") { ClinicId = Clinic2Id, TherapistId = C2T2Id },
                };
                context.Patients.AddRange(c2Patients);
                await context.SaveChangesAsync();
            }
        }
    }
}
