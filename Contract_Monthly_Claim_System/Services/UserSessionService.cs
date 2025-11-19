using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private const string UserIdSessionKey = "UserId";

        public UserSessionService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public ApplicationUser? GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32(UserIdSessionKey);
            if (!userId.HasValue)
                return null;

            return _context.Users
                .FirstOrDefault(u => u.Id == userId.Value && u.IsActive);
        }

        public void SetCurrentUser(int userId)
        {
            _httpContextAccessor.HttpContext?.Session.SetInt32(UserIdSessionKey, userId);
        }

        public void ClearCurrentUser()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(UserIdSessionKey);
        }
    }
}
