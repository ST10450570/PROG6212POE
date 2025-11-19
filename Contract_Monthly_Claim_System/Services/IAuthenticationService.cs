using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IAuthenticationService
    {
        Task<ApplicationUser?> AuthenticateAsync(string email, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
