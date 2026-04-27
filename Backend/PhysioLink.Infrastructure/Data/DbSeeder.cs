using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;

namespace PhysioLink.Infrastructure.Data
{
    public class DbSeeder
    {
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

            if (!context.Patients.Any())
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
                context.Patients.Add(patient);
                await context.SaveChangesAsync();

                var therapist = await context.Users.FirstAsync(u => u.Email == "fares.a.ibrahim@gmail.com");
                var exercises = await context.Exercises.ToListAsync();

                foreach (var exercise in exercises)
                {
                    context.ExerciseAssignments.Add(new ExerciseAssignment(
                        therapistId: therapist.ApplicationUserId,
                        patientId: patient.PatientId,
                        exerciseId: exercise.ExerciseId,
                        sets: exercise.Sets,
                        reps: exercise.Reps,
                        durationMinutes: exercise.DurationMinutes,
                        
                        assignedAt: DateTime.UtcNow
                    ));
                }
                await context.SaveChangesAsync();
            }
        }
    }
}