
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileEncryptionService _encryptionService;
        private readonly IUserSessionService _userSessionService;

        public DocumentsController(ApplicationDbContext context, IFileEncryptionService encryptionService, IUserSessionService userSessionService)
        {
            _context = context;
            _encryptionService = encryptionService;
            _userSessionService = userSessionService;
        }

        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Claim)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null) return NotFound();

            var claim = document.Claim;
            var currentUser = _userSessionService.GetCurrentUser();

            // Authorization check
            if (currentUser == null || (currentUser.Role == UserRole.Lecturer && claim?.UserId != currentUser.Id))
            {
                return Forbid();
            }

            try
            {
                var memoryStream = new MemoryStream();
                _encryptionService.DecryptBytesToStream(document.EncryptedContent, memoryStream);
                memoryStream.Position = 0;

                return File(memoryStream, document.ContentType, document.FileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error accessing file: {ex.Message}";
                return RedirectToAction("Details", "Claims", new { id = document.ClaimId });
            }
        }
    }
}