using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Microsoft.EntityFrameworkCore; // Required for FirstOrDefaultAsync

namespace Contract_Monthly_Claim_System.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, IAuthenticationService authService)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Define the users you want to guarantee exist
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    FullName = "Kekeletso Mokete",
                    Email = "hr@iiemsa.com",
                    PasswordHash = authService.HashPassword("Admin@123"),
                    Role = UserRole.HR,
                    Department = "Human Resources",
                    IsActive = true
                },
                new ApplicationUser
                {
                    FullName = "Chuma Makhathini",
                    Email = "chuma.makhathini@iiemsa.com",
                    PasswordHash = authService.HashPassword("Lecturer@123"),
                    Role = UserRole.Lecturer,
                    Department = "Computer Science",
                    HourlyRate = 350.00m,
                    IsActive = true
                },
                new ApplicationUser
                {
                    FullName = "Thabo Ndlovu",
                    Email = "thabo.ndlovu@iiemsa.com",
                    PasswordHash = authService.HashPassword("Lecturer@123"),
                    Role = UserRole.Lecturer,
                    Department = "Information Systems",
                    HourlyRate = 400.00m,
                    IsActive = true
                },
                new ApplicationUser
                {
                    FullName = "Nomsa Dlamini",
                    Email = "nomsa.dlamini@iiemsa.com",
                    PasswordHash = authService.HashPassword("Lecturer@123"),
                    Role = UserRole.Lecturer,
                    Department = "Software Engineering",
                    HourlyRate = 375.00m,
                    IsActive = true
                },
                new ApplicationUser
                {
                    FullName = "Muzi Sithole",
                    Email = "muzi.sithole@iiemsa.com",
                    PasswordHash = authService.HashPassword("Coord@123"),
                    Role = UserRole.Coordinator,
                    Department = "Head of Computer Science",
                    IsActive = true
                },
                new ApplicationUser
                {
                    FullName = "Ouma Stella",
                    Email = "ouma.stella@iiemsa.com",
                    PasswordHash = authService.HashPassword("Manager@123"),
                    Role = UserRole.Manager,
                    Department = "Head of School of IT",
                    IsActive = true
                }
            };

            // Iterate through each user to check if they exist
            foreach (var user in users)
            {
                var existingUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingUser == null)
                {
                    // User does not exist, add them
                    user.CreatedDate = DateTime.UtcNow;
                    user.UpdatedDate = DateTime.UtcNow;
                    await context.Users.AddAsync(user);
                }
                else
                {
                    // User exists, RESET the password to ensure login works
                    existingUser.PasswordHash = user.PasswordHash;

                    // Optional: Update other fields to match code
                    existingUser.Role = user.Role;
                    existingUser.HourlyRate = user.HourlyRate;
                    existingUser.UpdatedDate = DateTime.UtcNow;
                }
            }

            await context.SaveChangesAsync();

            Console.WriteLine("Database seeded/updated successfully!");
        }
    }
}