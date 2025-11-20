using Contract_Monthly_Claim_System.Validation;
using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.ViewModels
{
    public class ClaimCreateViewModel
    {
        [Required(ErrorMessage = "Work description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Module / Work Description")]
        public string WorkDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0.5, 180, ErrorMessage = "Hours must be between 0.5 and 180 per month.")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; }

        [StringLength(500)]
        [Display(Name = "Additional Notes (Optional)")]
        public string? Notes { get; set; }

        // Document is OPTIONAL
        [Display(Name = "Supporting Document (Optional - PDF, DOCX, XLSX)")]
        [MaxFileSize(5 * 1024 * 1024)] // 5MB max
        [AllowedExtensions(new string[] { ".pdf", ".docx", ".xlsx" })]
        public IFormFile? SupportingDocument { get; set; }
    }
}