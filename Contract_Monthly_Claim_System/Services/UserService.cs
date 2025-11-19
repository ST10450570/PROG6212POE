using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Models.ViewModel;
using Contract_Monthly_Claim_System.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthenticationService _authService;

        public UserService(ApplicationDbContext context, IAuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetLecturersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Lecturer && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<ApplicationUser> CreateUserAsync(UserCreateViewModel model)
        {
            // Check if email already exists
            if (await EmailExistsAsync(model.Email))
                throw new InvalidOperationException("A user with this email already exists.");

            // Validate hourly rate for lecturers
            if (model.Role == UserRole.Lecturer && !model.HourlyRate.HasValue)
                throw new InvalidOperationException("Hourly rate is required for lecturers.");

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email.ToLower(),
                PasswordHash = _authService.HashPassword(model.Password),
                Role = model.Role,
                Department = model.Department,
                HourlyRate = model.Role == UserRole.Lecturer ? model.HourlyRate : null,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<ApplicationUser> UpdateUserAsync(int id, UserEditViewModel model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            // Check if email is being changed and if it already exists
            if (user.Email.ToLower() != model.Email.ToLower() && await EmailExistsAsync(model.Email, id))
                throw new InvalidOperationException("A user with this email already exists.");

            // Validate hourly rate for lecturers
            if (model.Role == UserRole.Lecturer && !model.HourlyRate.HasValue)
                throw new InvalidOperationException("Hourly rate is required for lecturers.");

            user.FullName = model.FullName;
            user.Email = model.Email.ToLower();
            user.Role = model.Role;
            user.Department = model.Department;
            user.HourlyRate = model.Role == UserRole.Lecturer ? model.HourlyRate : null;
            user.IsActive = model.IsActive;
            user.UpdatedDate = DateTime.UtcNow;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = _authService.HashPassword(model.NewPassword);
            }

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            // Soft delete - just deactivate the user
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower() &&
                              (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
        }
    }
}