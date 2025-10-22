// ViewModels/ClaimCreateViewModel.cs
using Contract_Monthly_Claim_System.Validation;
using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.ViewModels
{
    public class ClaimCreateViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Module / Work Description")]
        public string ?WorkDescription { get; set; }

        [Required]
        [Range(0.5, 100, ErrorMessage = "Hours must be between 0.5 and 100.")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(50, 1000, ErrorMessage = "Rate must be between R50 and R1000.")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; } = 350; // Default Value

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Please upload a supporting document.")]
        [Display(Name = "Supporting Document")]
        [MaxFileSize(5 * 1024 * 1024)] // 5MB Limit
        [AllowedExtensions(new string[] { ".pdf", ".docx", ".xlsx" })]
        public IFormFile ?SupportingDocument { get; set; }
    }
}