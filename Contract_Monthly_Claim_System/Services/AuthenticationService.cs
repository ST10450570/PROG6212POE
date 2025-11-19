using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

            if (user == null)
                return null;

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        public string HashPassword(string password)
        {
            // Using BCrypt for password hashing
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}