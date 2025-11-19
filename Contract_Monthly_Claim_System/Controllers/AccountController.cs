using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Contract_Monthly_Claim_System.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserSessionService _userSessionService;

        public AccountController(IAuthenticationService authService, IUserSessionService userSessionService)
        {
            _authService = authService;
            _userSessionService = userSessionService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If user is already logged in, redirect to appropriate dashboard
            var currentUser = _userSessionService.GetCurrentUser();
            if (currentUser != null)
            {
                return RedirectToDashboard(currentUser.Role);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _authService.AuthenticateAsync(model.Email, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                // Set session
                _userSessionService.SetCurrentUser(user.Id);

                TempData["Success"] = $"Welcome back, {user.FullName}!";

                // Redirect based on return URL or user role
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToDashboard(user.Role);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred during login: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _userSessionService.ClearCurrentUser();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToDashboard(UserRole role)
        {
            return role switch
            {
                UserRole.Lecturer => RedirectToAction("Dashboard", "Claims"),
                UserRole.Coordinator => RedirectToAction("PendingCoordinator", "Claims"),
                UserRole.Manager => RedirectToAction("PendingManager", "Claims"),
                UserRole.HR => RedirectToAction("Dashboard", "HR"),
                _ => RedirectToAction("Login")
            };
        }
    }
}
