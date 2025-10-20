// Services/IUserSessionService.cs
using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IUserSessionService
    {
        ApplicationUser? GetCurrentUser();
        void SetCurrentUser(int userId);
        void ClearCurrentUser();
    }
}