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

        // Authorization Check
        private IActionResult CheckAuthorization(UserRole? requiredRole = null)
        {
            var currentUser = _userSessionService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (requiredRole.HasValue && currentUser.Role != requiredRole.Value)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return null!;
        }

        private ApplicationUser? GetCurrentUser()
        {
            return _userSessionService.GetCurrentUser();
        }

        // --- Dashboard Action ---
        public IActionResult Dashboard()
        {
            var authResult = CheckAuthorization(UserRole.Lecturer);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

            ViewBag.UserName = currentUser.FullName;
            ViewBag.Department = currentUser.Department;
            ViewBag.UserRole = currentUser.Role.ToString();
            ViewBag.UserInitials = currentUser.Initials;

            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetDashboardStats()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Role != UserRole.Lecturer)
                return Json(new { error = "Unauthorized" });

            try
            {
                var stats = new
                {
                    TotalClaims = await _claimService.GetTotalClaimsCountAsync(currentUser.Id),
                    ApprovedClaims = await _claimService.GetApprovedClaimsCountAsync(currentUser.Id),
                    PendingClaims = await _claimService.GetPendingClaimsCountAsync(currentUser.Id),
                    TotalAmount = await _claimService.GetTotalApprovedAmountAsync(currentUser.Id)
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRecentClaims()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Role != UserRole.Lecturer)
                return Json(new { error = "Unauthorized" });

            try
            {
                var recentClaims = await _claimService.GetRecentClaimsAsync(currentUser.Id, 10);
                var result = recentClaims.Select(c => new
                {
                    ClaimId = c.Id,
                    ClaimNumber = c.ClaimNumber,
                    WorkDescription = c.WorkDescription,
                    Period = c.CreatedDate.ToString("yyyy-MM"),
                    TotalAmount = c.TotalAmount,
                    Status = c.Status.ToString(),
                    ClaimDate = c.CreatedDate.ToString("yyyy-MM-dd")
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // --- Lecturer Actions ---
        public async Task<IActionResult> MyClaims()
        {
            var authResult = CheckAuthorization(UserRole.Lecturer);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;
            var claims = await _claimService.GetClaimsForUserAsync(currentUser.Id);
            return View(claims);
        }

        public IActionResult Create()
        {
            var authResult = CheckAuthorization(UserRole.Lecturer);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

            // Check if user has hourly rate set
            if (!currentUser.HourlyRate.HasValue || currentUser.HourlyRate.Value <= 0)
            {
                TempData["Error"] = "Your hourly rate has not been set by HR. Please contact HR before submitting claims.";
                return RedirectToAction("Dashboard");
            }

            var viewModel = new ClaimCreateViewModel
            {
                HourlyRate = currentUser.HourlyRate.Value // Pre-populate with user's rate
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimCreateViewModel viewModel)
        {
            var authResult = CheckAuthorization(UserRole.Lecturer);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var newClaim = await _claimService.CreateClaimAsync(viewModel, currentUser.Id);
                TempData["Success"] = $"Claim {newClaim.ClaimNumber} created successfully in Draft status.";
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
            var authResult = CheckAuthorization(UserRole.Lecturer);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

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

        // --- Edit Actions ---
        public async Task<IActionResult> Edit(int id)
        {
            var authResult = CheckAuthorization(UserRole.Lecturer);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;
            var claim = await _claimService.GetByIdAsync(id);

            if (claim == null) return NotFound();

            if (claim.UserId != currentUser.Id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.Returned)
            {
                TempData["Error"] = "You can only edit claims that are in draft or returned status.";
                return RedirectToAction("Details", new { id });
            }

            var viewModel = new ClaimEditViewModel
            {
                Id = claim.Id,
                WorkDescription = claim.WorkDescription,
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                Notes = claim.Notes,
                Status = claim.Status
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClaimEditViewModel viewModel)
        {
            var authResult = CheckAuthorization(UserRole.Lecturer);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                await _claimService.UpdateClaimAsync(id, viewModel, currentUser.Id);

                var successMessage = viewModel.Status == ClaimStatus.Returned
                    ? "Claim corrections saved successfully! You can now resubmit it for review."
                    : "Claim updated successfully!";

                TempData["Success"] = successMessage;
                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating the claim: {ex.Message}");
                return View(viewModel);
            }
        }

        // --- Coordinator Actions ---
        public async Task<IActionResult> PendingCoordinator()
        {
            var authResult = CheckAuthorization(UserRole.Coordinator);
            if (authResult != null) return authResult;

            var claims = await _claimService.GetPendingCoordinatorClaimsAsync();
            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id, string? comments)
        {
            var authResult = CheckAuthorization(UserRole.Coordinator);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

            try
            {
                await _claimService.VerifyClaimAsync(id, comments ?? string.Empty, currentUser.Id);
                TempData["Success"] = "Claim verified and escalated to Manager.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            // Check if request came from details page
            var referer = Request.Headers["Referer"].ToString();
            if (referer.Contains("/Claims/Details/"))
            {
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("PendingCoordinator");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, string comments)
        {
            var authResult = CheckAuthorization(UserRole.Coordinator);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

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

            // Check if request came from details page
            var referer = Request.Headers["Referer"].ToString();
            if (referer.Contains("/Claims/Details/"))
            {
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("PendingCoordinator");
        }

        // --- Manager Actions ---
        public async Task<IActionResult> PendingManager()
        {
            var authResult = CheckAuthorization(UserRole.Manager);
            if (authResult != null) return authResult;

            var claims = await _claimService.GetPendingManagerClaimsAsync();
            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? comments)
        {
            var authResult = CheckAuthorization(UserRole.Manager);
            if (authResult != null) return authResult;

            var currentUser = GetCurrentUser()!;

            try
            {
                await _claimService.ApproveClaimAsync(id, comments ?? string.Empty, currentUser.Id);
                TempData["Success"] = "Claim has been approved.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            // Check if request came from details page
            var referer = Request.Headers["Referer"].ToString();
            if (referer.Contains("/Claims/Details/"))
            {
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("PendingManager");
        }

        // --- Common Actions ---
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var claim = await _claimService.GetByIdAsync(id);
            if (claim == null) return NotFound();

            // Authorization: User can see their own claim, or reviewers/HR can see any claim
            if (currentUser.Role == UserRole.Lecturer && claim.UserId != currentUser.Id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Role == UserRole.Lecturer || currentUser.Role == UserRole.HR)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A reason is required to reject a claim.";

                var referer = Request.Headers["Referer"].ToString();
                if (referer.Contains("/Claims/Details/"))
                {
                    return RedirectToAction("Details", new { id });
                }

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

            // Check if request came from details page
            var referer2 = Request.Headers["Referer"].ToString();
            if (referer2.Contains("/Claims/Details/"))
            {
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction(currentUser.Role == UserRole.Coordinator ? "PendingCoordinator" : "PendingManager");
        }
    }
}