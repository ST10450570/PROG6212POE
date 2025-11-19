using System.ComponentModel.DataAnnotations;
using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Hourly Rate (For Lecturers)")]
        [Range(50, 1000, ErrorMessage = "Hourly rate must be between R50 and R1000")]
        public decimal? HourlyRate { get; set; }
    }

    
}