// Models/Claim.cs
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Contract_Monthly_Claim_System.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public required string ClaimNumber { get; set; }

        [Display(Name = "Work Description / Module")]
        public required string WorkDescription { get; set; }

        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }
        public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? SubmittedDate { get; set; }
        public string? RejectionReason { get; set; }
        public string? ReviewerComments { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Foreign Keys & Navigation Properties
        public int UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

        // Computed Properties for business logic
        public bool CanEdit => Status == ClaimStatus.Draft || Status == ClaimStatus.Returned;
        public bool CanSubmit => (Status == ClaimStatus.Draft || Status == ClaimStatus.Returned) && Documents.Any();
        public bool CanVerify => Status == ClaimStatus.Submitted;
        public bool CanApprove => Status == ClaimStatus.Verified;
        public bool CanReject => Status == ClaimStatus.Submitted || Status == ClaimStatus.Verified;
        public bool CanReturn => Status == ClaimStatus.Submitted;
    }
}