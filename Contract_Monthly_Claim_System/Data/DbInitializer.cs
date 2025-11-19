
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
            if (!context.Users.Any())
            {
                var users = new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        FullName = "Kekeletso Mokete",
                        Email = "hr@iiemsa.com",
                        PasswordHash = authService.HashPassword("Admin@123"),
                        Role = UserRole.HR,
                        Department = "Human Resources",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    },
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
                    new ApplicationUser
                    {
                        FullName = "Muzi Sithole",
                        Email = "muzi.sithole@iiemsa.com",
                        PasswordHash = authService.HashPassword("Coord@123"),
                        Role = UserRole.Coordinator,
                        Department = "Head of Computer Science",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    },
                    new ApplicationUser
                    {
                        FullName = "Ouma Stella",
                        Email = "ouma.stella@iiemsa.com",
                        PasswordHash = authService.HashPassword("Manager@123"),
                        Role = UserRole.Manager,
                        Department = "Head of School of IT",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }
    }
}