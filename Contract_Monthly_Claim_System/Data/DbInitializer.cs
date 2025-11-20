using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;

namespace Contract_Monthly_Claim_System.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, IAuthenticationService authService)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if users already exist
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            var users = new List<ApplicationUser>
            {
                // HR User
                new ApplicationUser
                {
                    FullName = "Kekeletso Mokete",
                    Email = "hr@iiemsa.com",
                    PasswordHash = authService.HashPassword("Admin@123"),
                    Role = UserRole.HR,
                    Department = "Human Resources",
                    HourlyRate = null,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                
                // Lecturer 1
                new ApplicationUser
                {
                    FullName = "Chuma Makhathini",
                    Email = "chuma.makhathini@iiemsa.com",
                    PasswordHash = authService.HashPassword("Lecturer@123"),
                    Role = UserRole.Lecturer,
                    Department = "Computer Science",
                    HourlyRate = 350.00m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                
                // Lecturer 2
                new ApplicationUser
                {
                    FullName = "Thabo Ndlovu",
                    Email = "thabo.ndlovu@iiemsa.com",
                    PasswordHash = authService.HashPassword("Lecturer@123"),
                    Role = UserRole.Lecturer,
                    Department = "Information Systems",
                    HourlyRate = 400.00m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                
                // Lecturer 3
                new ApplicationUser
                {
                    FullName = "Nomsa Dlamini",
                    Email = "nomsa.dlamini@iiemsa.com",
                    PasswordHash = authService.HashPassword("Lecturer@123"),
                    Role = UserRole.Lecturer,
                    Department = "Software Engineering",
                    HourlyRate = 375.00m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                
                // Programme Coordinator
                new ApplicationUser
                {
                    FullName = "Muzi Sithole",
                    Email = "muzi.sithole@iiemsa.com",
                    PasswordHash = authService.HashPassword("Coord@123"),
                    Role = UserRole.Coordinator,
                    Department = "Head of Computer Science",
                    HourlyRate = null,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                
                // Academic Manager
                new ApplicationUser
                {
                    FullName = "Ouma Stella",
                    Email = "ouma.stella@iiemsa.com",
                    PasswordHash = authService.HashPassword("Manager@123"),
                    Role = UserRole.Manager,
                    Department = "Head of School of IT",
                    HourlyRate = null,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            Console.WriteLine("Database seeded successfully!");
            Console.WriteLine("Default Credentials:");
            Console.WriteLine("HR: hr@iiemsa.com / Admin@123");
            Console.WriteLine("Lecturer: chuma.makhathini@iiemsa.com / Lecturer@123");
            Console.WriteLine("Coordinator: muzi.sithole@iiemsa.com / Coord@123");
            Console.WriteLine("Manager: ouma.stella@iiemsa.com / Manager@123");
        }
    }
}