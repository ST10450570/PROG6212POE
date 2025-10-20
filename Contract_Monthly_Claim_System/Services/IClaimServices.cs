// Services/IClaimService.cs
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.ViewModels;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IClaimService
    {
        Task<Claim?> GetByIdAsync(int claimId);
        Task<IEnumerable<Claim>> GetClaimsForUserAsync(int userId);
        Task<IEnumerable<Claim>> GetPendingCoordinatorClaimsAsync();
        Task<IEnumerable<Claim>> GetPendingManagerClaimsAsync();
        Task<Claim> CreateClaimAsync(ClaimCreateViewModel viewModel, int userId);
        Task SubmitClaimAsync(int claimId, int userId);
        Task VerifyClaimAsync(int claimId, string comments, int coordinatorId);
        Task ApproveClaimAsync(int claimId, string comments, int managerId);
        Task RejectClaimAsync(int claimId, string reason, int reviewerId);
        Task ReturnClaimAsync(int claimId, string comments, int coordinatorId);
    }
}