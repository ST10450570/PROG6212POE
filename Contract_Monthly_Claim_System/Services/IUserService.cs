using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.ViewModels;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<IEnumerable<ApplicationUser>> GetLecturersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(int id);
        Task<ApplicationUser> CreateUserAsync(UserCreateViewModel model);
        Task<ApplicationUser> UpdateUserAsync(int id, UserEditViewModel model);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> HardDeleteUserAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    }
}