// Controllers/HomeController.cs
using Contract_Monthly_Claim_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserSessionService _userSessionService;

        public HomeController(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetUser(int userId)
        {
            _userSessionService.SetCurrentUser(userId);
            var user = _userSessionService.GetCurrentUser();

            if (user == null) return RedirectToAction("Index");

            // Redirect user to their default dashboard
            return user.Role switch
            {
                Models.UserRole.Lecturer => RedirectToAction("Dashboard", "Claims"),
                Models.UserRole.Coordinator => RedirectToAction("PendingCoordinator", "Claims"),
                Models.UserRole.Manager => RedirectToAction("PendingManager", "Claims"),
                _ => RedirectToAction("Index")
            };
        }

        public IActionResult Logout()
        {
            _userSessionService.ClearCurrentUser();
            return RedirectToAction("Index");
        }
    }
}