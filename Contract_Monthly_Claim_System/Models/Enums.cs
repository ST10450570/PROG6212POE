// Models/Enums.cs
namespace Contract_Monthly_Claim_System.Models
{
    public enum UserRole
    {
        Lecturer,
        Coordinator,
        Manager
    }

    public enum ClaimStatus
    {
        Draft,
        Submitted,
        Verified,
        Approved,
        Rejected,
        Returned
    }
}