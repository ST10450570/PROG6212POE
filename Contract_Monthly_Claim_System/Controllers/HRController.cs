using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Models.ViewModel;
using Contract_Monthly_Claim_System.Services;
using Contract_Monthly_Claim_System.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class HRController : Controller
    {
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IUserSessionService _userSessionService;

        public HRController(IUserService userService, IClaimService claimService, IUserSessionService userSessionService)
        {
            _userService = userService;
            _claimService = claimService;
            _userSessionService = userSessionService;
        }

        // Authorization Helper
        private ApplicationUser? AuthorizeHR()
        {
            var user = _userSessionService.GetCurrentUser();
            if (user == null || user.Role != UserRole.HR)
            {
                return null;
            }
            return user;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            ViewBag.UserName = currentUser.FullName;
            ViewBag.UserRole = "HR Administrator";
            ViewBag.UserInitials = currentUser.Initials;

            var users = await _userService.GetAllUsersAsync();
            var claims = await _claimService.GetAllClaimsAsync();

            ViewBag.TotalUsers = users.Count();
            ViewBag.TotalLecturers = users.Count(u => u.Role == UserRole.Lecturer);
            ViewBag.TotalClaims = claims.Count();
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved);

            return View();
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate hourly rate for lecturers
            if (model.Role == UserRole.Lecturer && (!model.HourlyRate.HasValue || model.HourlyRate.Value <= 0))
            {
                ModelState.AddModelError("HourlyRate", "Hourly rate is required for lecturers.");
                return View(model);
            }

            try
            {
                var user = await _userService.CreateUserAsync(model);
                TempData["Success"] = $"User {user.FullName} created successfully. Login credentials have been set.";
                return RedirectToAction("Users");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Department = user.Department,
                HourlyRate = user.HourlyRate,
                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UserEditViewModel model)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate hourly rate for lecturers
            if (model.Role == UserRole.Lecturer && (!model.HourlyRate.HasValue || model.HourlyRate.Value <= 0))
            {
                ModelState.AddModelError("HourlyRate", "Hourly rate is required for lecturers.");
                return View(model);
            }

            try
            {
                await _userService.UpdateUserAsync(id, model);
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction("Users");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            // Prevent deactivating self
            if (id == currentUser.Id)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction("Users");
            }

            try
            {
                await _userService.DeleteUserAsync(id);
                TempData["Success"] = "User deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deactivating user: {ex.Message}";
            }

            return RedirectToAction("Users");
        }

        // Reports & Claims Management
        public async Task<IActionResult> Reports()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var approvedClaims = await _claimService.GetApprovedClaimsAsync();
            return View(approvedClaims);
        }

        public async Task<IActionResult> AllClaims()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var claims = await _claimService.GetAllClaimsAsync();
            return View(claims);
        }

        // Generate Invoice/Report (Simple CSV export)
        public async Task<IActionResult> GenerateReport(string reportType)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            try
            {
                if (reportType == "approved")
                {
                    var claims = await _claimService.GetApprovedClaimsAsync();
                    var csv = GenerateApprovedClaimsCSV(claims);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"ApprovedClaims_{DateTime.Now:yyyyMMdd}.csv");
                }
                else if (reportType == "all")
                {
                    var claims = await _claimService.GetAllClaimsAsync();
                    var csv = GenerateAllClaimsCSV(claims);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"AllClaims_{DateTime.Now:yyyyMMdd}.csv");
                }

                TempData["Error"] = "Invalid report type.";
                return RedirectToAction("Reports");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
                return RedirectToAction("Reports");
            }
        }

        private string GenerateApprovedClaimsCSV(IEnumerable<Claim> claims)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Claim Number,Lecturer Name,Email,Department,Hours Worked,Hourly Rate,Total Amount,Submitted Date,Approved By");

            foreach (var claim in claims)
            {
                csv.AppendLine($"\"{claim.ClaimNumber}\",\"{claim.User?.FullName}\",\"{claim.User?.Email}\",\"{claim.User?.Department}\",{claim.HoursWorked},{claim.HourlyRate},{claim.TotalAmount},\"{claim.SubmittedDate:yyyy-MM-dd}\",\"{claim.Manager?.FullName}\"");
            }

            return csv.ToString();
        }

        private string GenerateAllClaimsCSV(IEnumerable<Claim> claims)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Claim Number,Lecturer Name,Email,Status,Hours Worked,Total Amount,Created Date,Submitted Date");

            foreach (var claim in claims)
            {
                csv.AppendLine($"\"{claim.ClaimNumber}\",\"{claim.User?.FullName}\",\"{claim.User?.Email}\",\"{claim.Status}\",{claim.HoursWorked},{claim.TotalAmount},\"{claim.CreatedDate:yyyy-MM-dd}\",\"{claim.SubmittedDate:yyyy-MM-dd}\"");
            }

            return csv.ToString();
        }
    }
}