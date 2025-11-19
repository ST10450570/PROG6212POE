using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public int ClaimId { get; set; }

        // Navigation Property
        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }
    }
}
