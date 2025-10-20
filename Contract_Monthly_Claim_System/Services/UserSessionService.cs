// Services/UserSessionService.cs
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly InMemoryDataStore _dataStore;
        private const string UserIdSessionKey = "UserId";

        public UserSessionService(IHttpContextAccessor httpContextAccessor, InMemoryDataStore dataStore)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataStore = dataStore;
        }

        public ApplicationUser? GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32(UserIdSessionKey);
            return userId.HasValue ? _dataStore.Users.FirstOrDefault(u => u.Id == userId.Value) : null;
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