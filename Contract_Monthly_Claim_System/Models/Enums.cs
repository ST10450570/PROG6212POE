namespace Contract_Monthly_Claim_System.Models
{
    public enum UserRole
    {
        Lecturer = 0,
        Coordinator = 1,
        Manager = 2,
        HR = 3
    }

    public enum ClaimStatus
    {
        Draft = 0,
        Submitted = 1,
        Verified = 2,
        Approved = 3,
        Rejected = 4,
        Returned = 5
    }
}