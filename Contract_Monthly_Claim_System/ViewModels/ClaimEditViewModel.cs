// ViewModels/ClaimEditViewModel.cs
using System.ComponentModel.DataAnnotations;
using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.ViewModels
{
    public class ClaimEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Work description is required.")]
        [StringLength(1000, ErrorMessage = "Work description cannot exceed 1000 characters.")]
        [Display(Name = "Work Description")]
        public string WorkDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours worked is required.")]
        [Range(0.5, 200, ErrorMessage = "Hours worked must be between 0.5 and 200.")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required.")]
        [Range(1, 1000, ErrorMessage = "Hourly rate must be between R1 and R1000.")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }

        public ClaimStatus Status { get; set; }
    }
}