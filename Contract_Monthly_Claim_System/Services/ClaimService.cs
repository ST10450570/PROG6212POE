// Services/ClaimService.cs
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.ViewModels;

namespace Contract_Monthly_Claim_System.Services
{
    public class ClaimService : IClaimService
    {
        private readonly InMemoryDataStore _dataStore;
        private readonly IFileEncryptionService _encryptionService;

        public ClaimService(InMemoryDataStore dataStore, IFileEncryptionService encryptionService)
        {
            _dataStore = dataStore;
            _encryptionService = encryptionService;
        }

        public async Task<Claim?> GetByIdAsync(int claimId)
        {
            var claim = _dataStore.Claims.FirstOrDefault(c => c.Id == claimId);
            if (claim != null)
            {
                claim.User = _dataStore.Users.First(u => u.Id == claim.UserId);
                claim.Documents = _dataStore.Documents.Where(d => d.ClaimId == claim.Id).ToList();
            }
            return await Task.FromResult(claim);
        }

        public async Task<IEnumerable<Claim>> GetClaimsForUserAsync(int userId)
        {
            var claims = _dataStore.Claims
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            claims.ForEach(c => c.User = _dataStore.Users.First(u => u.Id == c.UserId));
            return await Task.FromResult(claims);
        }

        public async Task<IEnumerable<Claim>> GetPendingCoordinatorClaimsAsync()
        {
            var claims = _dataStore.Claims
                .Where(c => c.Status == ClaimStatus.Submitted)
                .OrderBy(c => c.SubmittedDate)
                .ToList();

            claims.ForEach(c => c.User = _dataStore.Users.First(u => u.Id == c.UserId));
            return await Task.FromResult(claims);
        }

        public async Task<IEnumerable<Claim>> GetPendingManagerClaimsAsync()
        {
            var claims = _dataStore.Claims
                .Where(c => c.Status == ClaimStatus.Verified)
                .OrderBy(c => c.SubmittedDate)
                .ToList();

            claims.ForEach(c => c.User = _dataStore.Users.First(u => u.Id == c.UserId));
            return await Task.FromResult(claims);
        }

        public async Task<Claim> CreateClaimAsync(ClaimCreateViewModel viewModel, int userId)
        {
            var user = _dataStore.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("User not found.");

            var newClaim = new Claim
            {
                Id = _dataStore.GetNextClaimId(),
                ClaimNumber = $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                WorkDescription = viewModel.WorkDescription,
                HoursWorked = viewModel.HoursWorked,
                HourlyRate = viewModel.HourlyRate,
                TotalAmount = viewModel.HoursWorked * viewModel.HourlyRate,
                Notes = viewModel.Notes,
                Status = ClaimStatus.Draft,
                UserId = userId,
                User = user
            };

            // Handle file encryption and storage
            byte[] encryptedData;
            using (var inputStream = viewModel.SupportingDocument.OpenReadStream())
            {
                encryptedData = _encryptionService.EncryptStreamToBytes(inputStream);
            }

            var newDocument = new Document
            {
                Id = _dataStore.GetNextDocumentId(),
                FileName = viewModel.SupportingDocument.FileName,
                ContentType = viewModel.SupportingDocument.ContentType,
                EncryptedContent = encryptedData,
                ClaimId = newClaim.Id
            };

            _dataStore.Documents.Add(newDocument);
            _dataStore.Claims.Add(newClaim);

            return await Task.FromResult(newClaim);
        }

        public async Task SubmitClaimAsync(int claimId, int userId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || claim.UserId != userId || !claim.CanSubmit)
                throw new InvalidOperationException("Claim not found or cannot be submitted.");

            claim.Status = ClaimStatus.Submitted;
            claim.SubmittedDate = DateTime.Now;
        }

        public async Task VerifyClaimAsync(int claimId, string comments, int coordinatorId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanVerify)
                throw new InvalidOperationException("Claim not found or cannot be verified.");

            claim.Status = ClaimStatus.Verified;
            claim.ReviewerComments = comments;
        }

        public async Task ApproveClaimAsync(int claimId, string comments, int managerId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanApprove)
                throw new InvalidOperationException("Claim not found or cannot be approved.");

            claim.Status = ClaimStatus.Approved;
            claim.ReviewerComments = comments;
        }

        public async Task RejectClaimAsync(int claimId, string reason, int reviewerId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanReject)
                throw new InvalidOperationException("Claim not found or cannot be rejected.");

            claim.Status = ClaimStatus.Rejected;
            claim.RejectionReason = reason;
        }

        public async Task ReturnClaimAsync(int claimId, string comments, int coordinatorId)
        {
            var claim = await GetByIdAsync(claimId);
            if (claim == null || !claim.CanReturn)
                throw new InvalidOperationException("Claim not found or cannot be returned.");

            claim.Status = ClaimStatus.Returned;
            claim.ReviewerComments = comments;
        }

        public async Task<int> GetTotalClaimsCountAsync(int userId)
        {
            return await Task.FromResult(
                _dataStore.Claims.Where(c => c.UserId == userId).Count()
            );
        }

        public async Task<int> GetApprovedClaimsCountAsync(int userId)
        {
            return await Task.FromResult(
                _dataStore.Claims.Where(c => c.UserId == userId && c.Status == ClaimStatus.Approved).Count()
            );
        }

        public async Task<int> GetPendingClaimsCountAsync(int userId)
        {
            return await Task.FromResult(
                _dataStore.Claims.Where(c => c.UserId == userId &&
                    (c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.Verified)).Count()
            );
        }

        public async Task<decimal> GetTotalApprovedAmountAsync(int userId)
        {
            return await Task.FromResult(
                _dataStore.Claims.Where(c => c.UserId == userId && c.Status == ClaimStatus.Approved)
                    .Sum(c => c.TotalAmount)
            );
        }

        public async Task<IEnumerable<Claim>> GetRecentClaimsAsync(int userId, int count = 10)
        {
            return await Task.FromResult(
                _dataStore.Claims.Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(count)
                    .ToList()
            );
        }

        public async Task UpdateClaimAsync(int claimId, ClaimEditViewModel viewModel, int userId)
        {
            var claim = _dataStore.Claims
                .FirstOrDefault(c => c.Id == claimId && c.UserId == userId);

            if (claim == null)
                throw new InvalidOperationException("Claim not found.");

            // Allow editing for both Draft and Returned status
            if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.Returned)
                throw new InvalidOperationException("Only draft and returned claims can be edited.");

            claim.WorkDescription = viewModel.WorkDescription;
            claim.HoursWorked = viewModel.HoursWorked;
            claim.HourlyRate = viewModel.HourlyRate;
            claim.Notes = viewModel.Notes;
            claim.TotalAmount = viewModel.HoursWorked * viewModel.HourlyRate;
            claim.UpdatedDate = DateTime.UtcNow;

            // If claim was returned and is being edited, change status back to Draft
            if (claim.Status == ClaimStatus.Returned)
            {
                claim.Status = ClaimStatus.Draft;
                // Clear previous reviewer comments since we're making corrections
                claim.ReviewerComments = null;
            }

            // No SaveChangesAsync needed for in-memory data store
            await Task.CompletedTask;
        }
    }
}