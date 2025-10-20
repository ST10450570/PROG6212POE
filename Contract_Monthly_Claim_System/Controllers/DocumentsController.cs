// Controllers/DocumentsController.cs
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly InMemoryDataStore _dataStore;
        private readonly IFileEncryptionService _encryptionService;
        private readonly IUserSessionService _userSessionService;

        public DocumentsController(InMemoryDataStore dataStore, IFileEncryptionService encryptionService, IUserSessionService userSessionService)
        {
            _dataStore = dataStore;
            _encryptionService = encryptionService;
            _userSessionService = userSessionService;
        }

        public IActionResult Download(int id)
        {
            var document = _dataStore.Documents.FirstOrDefault(d => d.Id == id);
            if (document == null) return NotFound();

            var claim = _dataStore.Claims.FirstOrDefault(c => c.Id == document.ClaimId);
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
                memoryStream.Position = 0; // Reset stream position to the beginning

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