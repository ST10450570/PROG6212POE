using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileEncryptionService _encryptionService;

        public ClaimService(ApplicationDbContext context, IFileEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task<Claim?> GetByIdAsync(int claimId)
        {
            return await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Include(c => c.Coordinator)
                .Include(c => c.Manager)
                .FirstOrDefaultAsync(c => c.Id == claimId);
        }

        public async Task<IEnumerable<Claim>> GetClaimsForUserAsync(int userId)
        {
            return await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetPendingCoordinatorClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Where(c => c.Status == ClaimStatus.Submitted)
                .OrderBy(c => c.SubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetPendingManagerClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Include(c => c.Coordinator)
                .Where(c => c.Status == ClaimStatus.Verified)
                .OrderBy(c => c.SubmittedDate)
                .ToListAsync();
        }

        public async Task<Claim> CreateClaimAsync(ClaimCreateViewModel viewModel, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.Lecturer)
                throw new InvalidOperationException("User not found or not authorized to create claims.");

            // Ensure lecturer has hourly rate set by HR
            if (!user.HourlyRate.HasValue || user.HourlyRate.Value <= 0)
                throw new InvalidOperationException("Your hourly rate has not been set by HR. Please contact HR to set your rate before submitting claims.");

            // Validate hours - maximum 180 hours per month
            if (viewModel.HoursWorked > 180)
                throw new InvalidOperationException("Hours worked cannot exceed 180 hours per month.");

            // Use the hourly rate from user profile (set by HR)
            var hourlyRate = user.HourlyRate.Value;

            var newClaim = new Claim
            {
                ClaimNumber = GenerateClaimNumber(),
                WorkDescription = viewModel.WorkDescription,
                HoursWorked = viewModel.HoursWorked,
                HourlyRate = hourlyRate, // Auto-pulled from user profile
                TotalAmount = viewModel.HoursWorked * hourlyRate,
                Notes = viewModel.Notes,
                Status = ClaimStatus.Draft,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Claims.Add(newClaim);
            await _context.SaveChangesAsync();

            // Handle document upload if provided (OPTIONAL)
            if (viewModel.SupportingDocument != null)
            {
                byte[] encryptedData;
                using (var inputStream = viewModel.SupportingDocument.OpenReadStream())
                {
                    encryptedData = _encryptionService.EncryptStreamToBytes(inputStream);
                }

                var newDocument = new Document
                {
                    FileName = viewModel.SupportingDocument.FileName,
                    ContentType = viewModel.SupportingDocument.ContentType,
                    EncryptedContent = encryptedData,
                    ClaimId = newClaim.Id,
                    UploadDate = DateTime.UtcNow
                };

                _context.Documents.Add(newDocument);
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(newClaim.Id)
                ?? throw new InvalidOperationException("Failed to retrieve created claim.");
        }

        public async Task SubmitClaimAsync(int claimId, int userId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || claim.UserId != userId)
                throw new InvalidOperationException("Claim not found or access denied.");

            if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.Returned)
                throw new InvalidOperationException("Only draft or returned claims can be submitted.");

            claim.Status = ClaimStatus.Submitted;
            claim.SubmittedDate = DateTime.UtcNow;
            claim.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task VerifyClaimAsync(int claimId, string comments, int coordinatorId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanVerify)
                throw new InvalidOperationException("Claim not found or cannot be verified.");

            claim.Status = ClaimStatus.Verified;
            claim.ReviewerComments = comments;
            claim.CoordinatorId = coordinatorId;
            claim.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ApproveClaimAsync(int claimId, string comments, int managerId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanApprove)
                throw new InvalidOperationException("Claim not found or cannot be approved.");

            claim.Status = ClaimStatus.Approved;
            claim.ReviewerComments = comments;
            claim.ManagerId = managerId;
            claim.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task RejectClaimAsync(int claimId, string reason, int reviewerId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanReject)
                throw new InvalidOperationException("Claim not found or cannot be rejected.");

            var currentStatus = claim.Status;
            claim.Status = ClaimStatus.Rejected;
            claim.RejectionReason = reason;
            claim.UpdatedDate = DateTime.UtcNow;

            // Set reviewer ID based on current status
            if (currentStatus == ClaimStatus.Submitted)
            {
                claim.CoordinatorId = reviewerId;
            }
            else if (currentStatus == ClaimStatus.Verified)
            {
                claim.ManagerId = reviewerId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ReturnClaimAsync(int claimId, string comments, int coordinatorId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanReturn)
                throw new InvalidOperationException("Claim not found or cannot be returned.");

            claim.Status = ClaimStatus.Returned;
            claim.ReviewerComments = comments;
            claim.CoordinatorId = coordinatorId;
            claim.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalClaimsCountAsync(int userId)
        {
            return await _context.Claims
                .Where(c => c.UserId == userId)
                .CountAsync();
        }

        public async Task<int> GetApprovedClaimsCountAsync(int userId)
        {
            return await _context.Claims
                .Where(c => c.UserId == userId && c.Status == ClaimStatus.Approved)
                .CountAsync();
        }

        public async Task<int> GetPendingClaimsCountAsync(int userId)
        {
            return await _context.Claims
                .Where(c => c.UserId == userId &&
                           (c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.Verified))
                .CountAsync();
        }

        public async Task<decimal> GetTotalApprovedAmountAsync(int userId)
        {
            return await _context.Claims
                .Where(c => c.UserId == userId && c.Status == ClaimStatus.Approved)
                .SumAsync(c => c.TotalAmount);
        }

        public async Task<IEnumerable<Claim>> GetRecentClaimsAsync(int userId, int count = 10)
        {
            return await _context.Claims
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task UpdateClaimAsync(int claimId, ClaimEditViewModel viewModel, int userId)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == claimId && c.UserId == userId);

            if (claim == null)
                throw new InvalidOperationException("Claim not found or access denied.");

            if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.Returned)
                throw new InvalidOperationException("Only draft and returned claims can be edited.");

            // Validate hours - maximum 180 hours per month
            if (viewModel.HoursWorked > 180)
                throw new InvalidOperationException("Hours worked cannot exceed 180 hours per month.");

            // Use the user's current hourly rate (as set by HR)
            var hourlyRate = claim.User?.HourlyRate ?? viewModel.HourlyRate;

            claim.WorkDescription = viewModel.WorkDescription;
            claim.HoursWorked = viewModel.HoursWorked;
            claim.HourlyRate = hourlyRate;
            claim.Notes = viewModel.Notes;
            claim.TotalAmount = viewModel.HoursWorked * hourlyRate;
            claim.UpdatedDate = DateTime.UtcNow;

            // If claim was returned and is being edited, change status back to Draft
            if (claim.Status == ClaimStatus.Returned)
            {
                claim.Status = ClaimStatus.Draft;
                claim.ReviewerComments = null;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Claim>> GetAllClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Coordinator)
                .Include(c => c.Manager)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetApprovedClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Manager)
                .Where(c => c.Status == ClaimStatus.Approved)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();
        }

        private string GenerateClaimNumber()
        {
            return $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}