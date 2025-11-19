using Contract_Monthly_Claim_System.Validation;
using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.ViewModels
{
    public class ClaimCreateViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Module / Work Description")]
        public string WorkDescription { get; set; } = string.Empty;

        [Required]
        [Range(0.5, 200, ErrorMessage = "Hours must be between 0.5 and 200.")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(50, 1000, ErrorMessage = "Rate must be between R50 and R1000.")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; } = 350;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Document is now OPTIONAL
        [Display(Name = "Supporting Document (Optional)")]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".pdf", ".docx", ".xlsx" })]
        public IFormFile? SupportingDocument { get; set; }
    }
}