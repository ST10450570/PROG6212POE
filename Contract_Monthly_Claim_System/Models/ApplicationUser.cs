using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [StringLength(200)]
        public string Department { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HourlyRate { get; set; } // Only for Lecturers

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Computed Property
        [NotMapped]
        public string Initials => string.Concat(FullName.Split(' ').Select(n => n.FirstOrDefault())).ToUpper();

        // Navigation Properties
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public virtual ICollection<Claim> CoordinatedClaims { get; set; } = new List<Claim>();
        public virtual ICollection<Claim> ManagedClaims { get; set; } = new List<Claim>();
    }
}