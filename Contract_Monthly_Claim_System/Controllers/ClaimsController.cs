// Controllers/ClaimsController.cs
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Contract_Monthly_Claim_System.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly IUserSessionService _userSessionService;

        public ClaimsController(IClaimService claimService, IUserSessionService userSessionService)
        {
            _claimService = claimService;
            _userSessionService = userSessionService;
        }

        // --- Lecturer Actions ---
        public async Task<IActionResult> MyClaims()
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Lecturer);
            if (currentUser == null) return Forbid();

            var claims = await _claimService.GetClaimsForUserAsync(currentUser.Id);
            return View(claims);
        }

        public IActionResult Create()
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Lecturer);
            if (currentUser == null) return Forbid();

            return View(new ClaimCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimCreateViewModel viewModel)
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Lecturer);
            if (currentUser == null) return Forbid();

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var newClaim = await _claimService.CreateClaimAsync(viewModel, currentUser.Id);
                TempData["Success"] = $"Claim {newClaim.ClaimNumber} created successfully in Draft status. You can now submit it for review.";
                return RedirectToAction("Details", new { id = newClaim.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the claim: {ex.Message}");
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Lecturer);
            if (currentUser == null) return Forbid();

            try
            {
                await _claimService.SubmitClaimAsync(id, currentUser.Id);
                TempData["Success"] = "Claim submitted successfully for review!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Details", new { id });
        }

        // --- Coordinator Actions ---
        public async Task<IActionResult> PendingCoordinator()
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Coordinator);
            if (currentUser == null) return Forbid();

            var claims = await _claimService.GetPendingCoordinatorClaimsAsync();
            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id, string comments)
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Coordinator);
            if (currentUser == null) return Forbid();

            try
            {
                await _claimService.VerifyClaimAsync(id, comments, currentUser.Id);
                TempData["Success"] = "Claim verified and escalated to Manager.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("PendingCoordinator");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, string comments)
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Coordinator);
            if (currentUser == null) return Forbid();

            if (string.IsNullOrWhiteSpace(comments))
            {
                TempData["Error"] = "Correction instructions are required to return a claim.";
                return RedirectToAction("PendingCoordinator");
            }

            try
            {
                await _claimService.ReturnClaimAsync(id, comments, currentUser.Id);
                TempData["Success"] = "Claim returned to lecturer for correction.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("PendingCoordinator");
        }

        // --- Manager Actions ---
        public async Task<IActionResult> PendingManager()
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Manager);
            if (currentUser == null) return Forbid();

            var claims = await _claimService.GetPendingManagerClaimsAsync();
            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string comments)
        {
            var currentUser = AuthorizeAndGetUser(UserRole.Manager);
            if (currentUser == null) return Forbid();

            try
            {
                await _claimService.ApproveClaimAsync(id, comments, currentUser.Id);
                TempData["Success"] = "Claim has been approved.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("PendingManager");
        }

        // --- Common Actions ---
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = _userSessionService.GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var claim = await _claimService.GetByIdAsync(id);
            if (claim == null) return NotFound();

            // Authorization: User can see their own claim, or a reviewer can see any claim.
            if (claim.UserId != currentUser.Id && currentUser.Role == UserRole.Lecturer)
            {
                return Forbid();
            }

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var currentUser = _userSessionService.GetCurrentUser();
            if (currentUser == null || currentUser.Role == UserRole.Lecturer) return Forbid();

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A reason is required to reject a claim.";
                return RedirectToAction(currentUser.Role == UserRole.Coordinator ? "PendingCoordinator" : "PendingManager");
            }

            try
            {
                await _claimService.RejectClaimAsync(id, reason, currentUser.Id);
                TempData["Success"] = "Claim has been rejected.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(currentUser.Role == UserRole.Coordinator ? "PendingCoordinator" : "PendingManager");
        }

        // --- Private Helper Methods ---
        private ApplicationUser? AuthorizeAndGetUser(UserRole requiredRole)
        {
            var user = _userSessionService.GetCurrentUser();
            if (user == null || user.Role != requiredRole)
            {
                // In a real app, you'd redirect to a proper login or access denied page.
                // For this simulation, we return null, and the action returns Forbid().
                return null;
            }
            return user;
        }
    }
}