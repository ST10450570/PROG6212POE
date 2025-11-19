using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Work Description / Module")]
        public string WorkDescription { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.5, 200)]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedDate { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? RejectionReason { get; set; }

        [StringLength(1000)]
        public string? ReviewerComments { get; set; }

        // Foreign Keys
        [Required]
        public int UserId { get; set; }

        public int? CoordinatorId { get; set; }

        public int? ManagerId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("CoordinatorId")]
        public virtual ApplicationUser? Coordinator { get; set; }

        [ForeignKey("ManagerId")]
        public virtual ApplicationUser? Manager { get; set; }

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

        // Computed Properties
        [NotMapped]
        public bool CanEdit => Status == ClaimStatus.Draft || Status == ClaimStatus.Returned;

        [NotMapped]
        public bool CanSubmit => (Status == ClaimStatus.Draft || Status == ClaimStatus.Returned);

        [NotMapped]
        public bool CanVerify => Status == ClaimStatus.Submitted;

        [NotMapped]
        public bool CanApprove => Status == ClaimStatus.Verified;

        [NotMapped]
        public bool CanReject => Status == ClaimStatus.Submitted || Status == ClaimStatus.Verified;

        [NotMapped]
        public bool CanReturn => Status == ClaimStatus.Submitted;
    }
}